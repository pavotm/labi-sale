using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maze;

namespace GrowingTree
{
    interface IDataCell
    {
        bool Visited { get; set; }
    }

    class GrowingTree<DataCell, DataWall> where DataCell : IDataCell
    {
        static public void generate(List<Cell<DataCell, DataWall>> cells)
        {
            foreach (Cell<DataCell, DataWall> cell in cells)
            {
                cell.Data.Visited = false;
            }

            Random rng = new Random();

            Stack<Cell<DataCell, DataWall>> list = new Stack<Cell<DataCell, DataWall>>(cells.Count);
            list.Push(PickRandomCellAndMarkIt(cells, rng));

            while (list.Count > 0)
            {
                int n = NumberCloseWallToUnvisitedCell(list.Peek());
                if (n == 0)
                {
                    list.Pop();
                }
                else
                {
                    Cell<DataCell, DataWall> cell = OpenWallToUnvisitedCellAndMarkThem(list.Peek(), rng.Next(n));
                    list.Push(cell);
                }
            }
        }

        static private Cell<DataCell, DataWall> PickRandomCellAndMarkIt(List<Cell<DataCell, DataWall>> cells, Random rng)
        {
            Cell<DataCell, DataWall> cell = cells[rng.Next(cells.Count())];
            cell.Data.Visited = true;
            return cell;
        }

        static private int NumberCloseWallToUnvisitedCell(Cell<DataCell, DataWall> cell)
        {
            int n = 0;

            foreach (Wall<DataCell, DataWall> wall in cell.Walls)
            {
                if (wall.Open == false)
                {
                    Cell<DataCell, DataWall> other = cell.OtherCell(wall);
                    if (other.Data.Visited == false)
                    {
                        n++;
                    }
                }
            }

            return n;
        }

        static public Cell<DataCell, DataWall> OpenWallToUnvisitedCellAndMarkThem(Cell<DataCell, DataWall> cell, int n)
        {
            foreach (Wall<DataCell, DataWall> wall in cell.Walls)
            {
                if (wall.Open == false)
                {
                    Cell<DataCell, DataWall> other = cell.OtherCell(wall);
                    if (other.Data.Visited == false)
                    {
                        if (n == 0)
                        {
                            wall.Open = true;
                            other.Data.Visited = true;
                            return other;
                        }
                        n--;
                    }

                }
            }

            return null;
        }
    }
}
