using System;
using System.Text;
using UnityEngine;

public static class Utilities
{
    private static readonly StringBuilder MatrixBuilder = new StringBuilder(256);

    public static string[,] GetMatrixFromResourcesData(int rows, int columns)
    {
        string[,] shapes = new string[rows, columns];

        TextAsset txt = Resources.Load<TextAsset>("debugLevel");
        if(txt == null)
        {
            Debug.LogError("[Utilities] Файл 'debugLevel' не найден в Resources!");
            return shapes;
        }

        string text = txt.text.Replace("\r", string.Empty);
        string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for(int row = 0; row < rows && row < lines.Length; row++)
        {
            string[] items = lines[row].Split('|');
            for(int column = 0; column < columns && column < items.Length; column++)
            {
                shapes[row, column] = items[column];
            }
        }

        return shapes;
    }

    public static string ShowMatrixOnConsole(ItemArray matrix)
    {
        if(matrix == null) return string.Empty;

        MatrixBuilder.Clear();

        int rows = matrix.Rows;
        int columns = matrix.Columns;

        for(int row = rows - 1; row >= 0; row--)
        {
            for(int column = 0; column < columns; column++)
            {
                Item item = matrix[row, column];
                if(item != null)
                {
                    MatrixBuilder.Append(item.Value).Append('|');
                }
                else
                {
                    MatrixBuilder.Append("X|");
                }
            }
            MatrixBuilder.AppendLine();
        }

        string result = MatrixBuilder.ToString();
        Debug.Log(result);
        return result;
    }
}