using System.Collections.Generic;
using UnityEngine;

public class ItemArray
{
    private readonly Item[] matrix;
    private readonly int rows;
    private readonly int columns;

    private readonly List<ItemMovementDetails> movementDetailsBuffer = new List<ItemMovementDetails>(16);

    public int Rows => rows;
    public int Columns => columns;

    public ItemArray(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        this.matrix = new Item[rows * columns];
    }

    public Item this[int row, int column]
    {
        get => matrix[row * columns + column];
        set => matrix[row * columns + column] = value;
    }

    public void GetRandomRowColumn(out int row, out int column)
    {
        int freeSlots = 0;
        for(int i = 0; i < matrix.Length; i++)
        {
            if (matrix[i] == null) freeSlots++;
        }

        if (freeSlots == 0)
        {
            row = -1;
            column = -1;
            return;
        }

        do
        {
            row = Random.Range(0, rows);
            column = Random.Range(0, columns);
        } 
        while (this[row, column] != null);
    }

    public List<ItemMovementDetails> MoveHorizontal(HorizontalMovement horizontalMovement)
    {
        ResetWasJustDuplicatedValues();
        movementDetailsBuffer.Clear();

        bool isLeft = horizontalMovement == HorizontalMovement.Left;
        int relativeColumn = isLeft ? -1 : 1;

        for(int row = rows - 1; row >= 0; row--)
        {
            int startCol = isLeft ? 0 : columns - 1;
            int endCol = isLeft ? columns : -1;
            int stepCol = isLeft ? 1 : -1;

            for(int column = startCol; column != endCol; column += stepCol)
            {
                if(this[row, column] == null) continue;

                ItemMovementDetails imd = AreTheseTwoItemsSame(row, column, row, column + relativeColumn);
                if(imd != null)
                {
                    movementDetailsBuffer.Add(imd);
                    continue;
                }

                int columnFirstNullItem = -1;
                bool emptyItemFound = false;

                int searchStart = isLeft ? 0 : columns - 1;
                int searchEnd = column;
                int searchStep = isLeft ? 1 : -1;

                for (int tempCol = searchStart; tempCol != searchEnd; tempCol += searchStep)
                {
                    if (this[row, tempCol] == null)
                    {
                        columnFirstNullItem = tempCol;
                        emptyItemFound = true;
                        break;
                    }
                }

                if (!emptyItemFound) continue;

                ItemMovementDetails newImd = MoveItemToNullPositionAndCheckIfSameWithNextOne(
                    row, row, row, 
                    column, columnFirstNullItem, columnFirstNullItem + relativeColumn
                );

                movementDetailsBuffer.Add(newImd);
            }
        }

        return movementDetailsBuffer;
    }

    public List<ItemMovementDetails> MoveVertical(VerticalMovement verticalMovement)
    {
        ResetWasJustDuplicatedValues();
        movementDetailsBuffer.Clear();

        bool isBottom = verticalMovement == VerticalMovement.Down;
        int relativeRow = isBottom ? -1 : 1;

        for(int column = 0; column < columns; column++)
        {
            int startRow = isBottom ? 0 : rows - 1;
            int endRow = isBottom ? rows : -1;
            int stepRow = isBottom ? 1 : -1;

            for(int row = startRow; row != endRow; row += stepRow)
            {
                if(this[row, column] == null) continue;

                ItemMovementDetails imd = AreTheseTwoItemsSame(row, column, row + relativeRow, column);
                if(imd != null)
                {
                    movementDetailsBuffer.Add(imd);
                    continue;
                }

                int rowFirstNullItem = -1;
                bool emptyItemFound = false;

                int searchStart = isBottom ? 0 : rows - 1;
                int searchEnd = row;
                int searchStep = isBottom ? 1 : -1;

                for (int tempRow = searchStart; tempRow != searchEnd; tempRow += searchStep)
                {
                    if (this[tempRow, column] == null)
                    {
                        rowFirstNullItem = tempRow;
                        emptyItemFound = true;
                        break;
                    }
                }

                if (!emptyItemFound) continue;

                ItemMovementDetails newImd = MoveItemToNullPositionAndCheckIfSameWithNextOne(
                    row, rowFirstNullItem, rowFirstNullItem + relativeRow, 
                    column, column, column
                );

                movementDetailsBuffer.Add(newImd);
            }
        }

        return movementDetailsBuffer;
    }

    private ItemMovementDetails MoveItemToNullPositionAndCheckIfSameWithNextOne(
        int oldRow, int newRow, int itemToCheckRow, 
        int oldColumn, int newColumn, int itemToCheckColumn)
    {
        this[newRow, newColumn] = this[oldRow, oldColumn];
        this[oldRow, oldColumn] = null;

        ItemMovementDetails imd2 = AreTheseTwoItemsSame(newRow, newColumn, itemToCheckRow, itemToCheckColumn);
        if(imd2 != null)
        {
            return imd2;
        }

        return new ItemMovementDetails(newRow, newColumn, this[newRow, newColumn].GO, null);
    }

    private ItemMovementDetails AreTheseTwoItemsSame(
        int originalRow, int originalColumn, int toCheckRow, int toCheckColumn)
    {
        if(toCheckRow < 0 || toCheckColumn < 0 || toCheckRow >= rows || toCheckColumn >= columns)
            return null;

        Item itemOriginal = this[originalRow, originalColumn];
        Item itemToCheck = this[toCheckRow, toCheckColumn];

        if(itemOriginal != null && itemToCheck != null
            && itemOriginal.Value == itemToCheck.Value
            && !itemToCheck.WasJustDuplicated)
        {
            itemToCheck.Value *= 2;
            itemToCheck.WasJustDuplicated = true;

            var GOToAnimateScaleCopy = itemOriginal.GO;
            this[originalRow, originalColumn] = null;

            return new ItemMovementDetails(toCheckRow, toCheckColumn, itemToCheck.GO, GOToAnimateScaleCopy);
        }

        return null;
    }

    private void ResetWasJustDuplicatedValues()
    {
        for(int i = 0; i < matrix.Length; i++)
        {
            if(matrix[i] != null && matrix[i].WasJustDuplicated)
            {
                matrix[i].WasJustDuplicated = false;
            }
        }
    }
}

public enum HorizontalMovement { Left, Right }
public enum VerticalMovement { Up, Down }