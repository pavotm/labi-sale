using System;
using System.Collections.Generic;
using System.Linq;

namespace Maze
{
    public class Wall<DataCell, DataWall>
    {
        public DataWall Data { get; set; }
        public Cell<DataCell, DataWall> First { get; set; }
        public Cell<DataCell, DataWall> Second { get; set; }
        public bool Open { get; set; }

        public Wall(Cell<DataCell, DataWall> first, Cell<DataCell, DataWall> second, bool open, DataWall data)
        {
            First = first;
            Second = second;
            Open = open;
            First.Walls.Add(this);
            Second.Walls.Add(this);
            Data = data;
        }
    }

    public class Cell<DataCell, DataWall>
    {
        public DataCell Data { get; set; }
        public List<Wall<DataCell, DataWall>> Walls { get; }

        public Cell(List<Wall<DataCell, DataWall>> walls, DataCell data)
        {
            Data = data;
            Walls = walls;
        }

        public int NumberCloseWall()
        {
            int n = 0;

            foreach (Wall<DataCell, DataWall> wall in Walls)
            {
                if (wall.Open == false)
                {
                    n++;
                }
            }

            return n;
        }

        public Cell<DataCell, DataWall> OpenWall(int n)
        {
            foreach (Wall<DataCell, DataWall> wall in Walls)
            {
                if (wall.Open == false)
                {
                    Cell<DataCell, DataWall> cell = OtherCell(wall);
                    if (n == 0)
                    {
                        wall.Open = true;
                        return cell;
                    }
                    n--;
                }
            }

            return null;
        }

        public Cell<DataCell, DataWall> OtherCell(Wall<DataCell, DataWall> wall)
        {
            return wall.First == this ? wall.Second : wall.First;
        }

        public bool IsOpenWith(Cell<DataCell, DataWall> other)
        {
            foreach (Wall<DataCell, DataWall> wall in Walls)
            {
                if (OtherCell(wall) == other)
                {
                    return wall.Open;
                }
            }
            return false;
        }
    }
}