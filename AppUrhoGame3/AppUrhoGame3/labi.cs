﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUrhoGame3
{
    class labi
    {
        private int x = 0;
        private int y = 0;

        List<labicase> map;

        enum E_Etat
        {
            wall = 0,
            path = 1
        };

        public labi(int paramx, int paramy)
        {
            x = paramx;
            y = paramy;
            map = new List<labicase>(x * y);
        }


        public void generateLabicase()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    map.Add(new labicase(E_Etat.wall, E_Etat.wall, E_Etat.wall, E_Etat.wall, i * x + j));
                }
            }
        }

        public void generatelabi()
        {
            Random rng = new Random();
            int tot = x * y;

            labicase jonc = null;
            labicase open = null;

            int id = 0;
            int bord = 0;

            while (verifColor() == true)
            {
                id = rng.Next(0, tot);
                open = map[id];
                bord = rng.Next(0, 3);
                switch (bord)
                {
                    case 0:
                        jonc = getNordLabicase(open);
                        if (jonc != null && jonc.color != open.color)
                        {
                            jonc.bot = E_Etat.path;
                            open.top = E_Etat.path;
                            extendColor(open.color, jonc.color);
                        }
                        break;
                    case 1:
                        jonc = getEstLabicase(open);
                        if (jonc != null && jonc.color != open.color)
                        {
                            jonc.left = E_Etat.path;
                            open.right = E_Etat.path;
                            extendColor(open.color, jonc.color);
                        }
                        break;
                    case 2:
                        jonc = getSudLabicase(open);
                        if (jonc != null && jonc.color != open.color)
                        {
                            jonc.top = E_Etat.path;
                            open.bot = E_Etat.path;
                            extendColor(open.color, jonc.color);
                        }
                        break;
                    case 3:
                        jonc = getOuestLabicase(open);
                        if (jonc != null && jonc.color != open.color)
                        {
                            jonc.right = E_Etat.path;
                            open.left = E_Etat.path;
                            extendColor(open.color, jonc.color);
                        }
                        break;
                }
            }
        }

        private void extendColor(int baseColor, int setColor)
        {
            foreach (labicase lb in map)
            {
                if (lb.color == setColor)
                    lb.color = baseColor;
            }
        }


        public void printLabi()
        {
            string line = "";

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    //if (map[i * x + j].left == E_Etat.wall)
                    //    line += "|";
                    //else
                    //    line += " ";
                    if (map[i * x + j].bot == E_Etat.wall)
                        line += "_";
                    else
                        line += " ";
                    if (map[i * x + j].right == E_Etat.wall)
                        line += "|";
                    else
                        line += " ";

                }
                System.Diagnostics.Debug.WriteLine(line);
                line = "";
            }
        }


        private labicase getEstLabicase(labicase lb)
        {
            if (x - 1 == lb.id % x)
                return null;
            return map[lb.id + 1];
        }

        private labicase getOuestLabicase(labicase lb)
        {
            if (0 == (lb.id % x))
                return null;
            return map[lb.id - 1];
        }

        private labicase getNordLabicase(labicase lb)
        {
            if (lb.id < x)
                return null;
            return map[lb.id - x];
        }

        private labicase getSudLabicase(labicase lb)
        {
            if (lb.id >= x * y - x)
                return null;
            return map[lb.id + x];
        }


        private bool verifColor()
        {
            int ret = map.First().color;

            foreach (labicase lb in map)
            {
                if (lb.color != ret)
                    return true;
            }
            return false;
        }



        class labicase
        {
            public E_Etat top { get; set; }
            public E_Etat bot { get; set; }
            public E_Etat right { get; set; }
            public E_Etat left { get; set; }

            public int color { get; set; }
            public int id { get; set; }

            public labicase(E_Etat paramtop, E_Etat parambot, E_Etat paramleft, E_Etat paramright, int _id)
            {
                top = paramtop;
                bot = parambot;
                right = paramright;
                left = paramleft;
                id = _id;
                color = _id;
            }


        }
    }
}