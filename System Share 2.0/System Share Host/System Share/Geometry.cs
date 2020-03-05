using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System_Share
{
    class Geometry
    {
        /// <summary>
        /// Inserts disp into list to a valid position
        /// </summary>
        public static void Insert(Display disp, ref List<Display> list)
        {
            list.Add(disp);
            while (!Valid(list.Count - 1, list))
            {
                list[list.Count - 1].Area.X--;
            }
        }

        /// <summary>
        /// Arranges all rectangles untill there is only one poligon left, smaler poligons are a priority
        /// </summary>
        public static void ArrangeAll(ref List<Display> list)
        {
            int poligons;
            ResetPoligons(ref list);
            poligons = MakePoligons(ref list);
            while (poligons > 1)
            {
                int smalest = GetSmalestPoligon(list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].poligon == smalest)
                    {
                        MoveTo(i, GetClosest(i, list), ref list);
                    }
                }
                ResetPoligons(ref list);
                poligons = MakePoligons(ref list);
            }
        }

        /// <summary>
        /// moves the target close to the destination
        /// </summary>
        private static void MoveTo(int target, int destination, ref List<Display> list)
        {
            int ox = list[target].Area.X + list[target].Area.Width / 2;
            int oy = list[target].Area.Y + list[target].Area.Height / 2;
            int dx = list[destination].Area.X + list[destination].Area.Width / 2;
            int dy = list[destination].Area.Y + list[destination].Area.Height / 2;

            if (ox > list[destination].Area.Left && ox < list[destination].Area.Right)
            {
                if (oy < dy)
                {
                    //North
                    FollowFunction(target, ref list, ox, oy, ox, dy, 1, false);
                }
                else
                {
                    //South
                    FollowFunction(target, ref list, ox, oy, ox, dy, -1, false);
                }
            }
            else if (oy > list[destination].Area.Top && oy < list[destination].Area.Bottom)
            {
                if (ox < dx)
                {
                    //West
                    FollowFunction(target, ref list, oy, ox, oy, dx, 1, true);
                }
                else
                {
                    FollowFunction(target, ref list, oy, ox, oy, dx, -1, true);
                    //East
                }
            }
            else
            {
                int fx = list[destination].Area.X + list[destination].Area.Width;
                int fy = list[destination].Area.Y;
                int gx = list[destination].Area.X;
                int gy = list[destination].Area.Y;

                int fresult = ((dy - fy) * (ox - fx) + fy * (dx - fx)) / (dx - fx);
                int gresult = ((dy - gy) * (ox - gx) + gy * (dx - gx)) / (dx - gx);

                if (oy < fresult)
                {
                    if (oy < gresult)
                    {
                        //North diagonal (N.N.W. or N.N.E.)
                        FollowFunction(target, ref list, ox, oy, dx, dy, 1, false);
                    }
                    else
                    {
                        //West diagonal (W.W.N. or W.W.S.)
                        FollowFunction(target, ref list, oy, ox, dy, dx, 1, true);
                    }
                }
                else
                {
                    if (oy < gresult)
                    {
                        //East diagonal (E.E.N. or E.E.S.)
                        FollowFunction(target, ref list, oy, ox, dy, dx, -1, true);
                    }
                    else
                    {
                        //South diagonal (S.S.W. or S.S.E.)
                        FollowFunction(target, ref list, ox, oy, dx, dy, -1, false);
                    }
                }
            }

        }

        /// <summary>
        /// makes the target follow the given function while valid
        /// </summary>
        private static void FollowFunction(int target, ref List<Display> list, int StartResult, int StartVariable, int DestinationResult, int DestinationVariable, int direction, bool isXVariable)
        {
            do
            {
                if (isXVariable)
                {
                    list[target].Area.X += direction;
                    list[target].Area.Y = ((StartResult - DestinationResult) * (list[target].Area.X + list[target].Area.Width / 2 - DestinationVariable) + DestinationResult * (StartVariable - DestinationVariable)) / (StartVariable - DestinationVariable) - list[target].Area.Height / 2;
                }
                else
                {
                    list[target].Area.Y += direction;
                    list[target].Area.X = ((StartResult - DestinationResult) * (list[target].Area.Y + list[target].Area.Height / 2 - DestinationVariable) + DestinationResult * (StartVariable - DestinationVariable)) / (StartVariable - DestinationVariable) - list[target].Area.Width / 2;
                }
            } while (Valid(target, list));
            if (isXVariable)
            {
                list[target].Area.X -= direction;
                list[target].Area.Y = ((StartResult - DestinationResult) * (list[target].Area.X + list[target].Area.Width / 2 - DestinationVariable) + DestinationResult * (StartVariable - DestinationVariable)) / (StartVariable - DestinationVariable) - list[target].Area.Height / 2;
            }
            else
            {
                list[target].Area.Y -= direction;
                list[target].Area.X = ((StartResult - DestinationResult) * (list[target].Area.Y + list[target].Area.Height / 2 - DestinationVariable) + DestinationResult * (StartVariable - DestinationVariable)) / (StartVariable - DestinationVariable) - list[target].Area.Width / 2;
            }
        }

        /// <summary>
        /// Validates the item at index by checking if it colided with anything
        /// </summary>
        public static bool Valid(int index, List<Display> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i != index)
                {
                    Rectangle rec = Rectangle.Intersect(list[index].Area, list[i].Area);
                    if (rec.Width > 1 && rec.Height > 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// if the item has poligon not -1, generate a poligon with it, returns number of poligons
        /// </summary>
        private static int MakePoligons(ref List<Display> list)
        {
            int j = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].poligon == -1)
                {
                    list[i].GeneratePoligon(j, ref list);
                    j++;
                }
            }
            return j;
        }

        /// <summary>
        /// Returns the index of the smalest poligon
        /// </summary>
        private static int GetSmalestPoligon(List<Display> list)
        {
            var dict = new Dictionary<int, int>();
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    dict[list[i].poligon]++;
                }
                catch (Exception)
                {
                    dict.Add(list[i].poligon, 1);
                }
            }
            int min = -1;
            foreach (var i in dict)
            {
                if (min == -1)
                {
                    min = i.Key;
                }
                else if (dict[min] > i.Value)
                {
                    min = i.Key;
                }
            }
            return min;
        }

        /// <summary>
        /// Sets every item's poligon to -1
        /// </summary>
        /// <param name="list"></param>
        private static void ResetPoligons(ref List<Display> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].poligon = -1;
            }
        }

        /// <summary>
        /// returns the index of the closest rectangle to the one provided
        /// </summary>
        private static int GetClosest(int index, List<Display> list)
        {
            int min = -1;
            int distance = -1;
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].poligon != list[index].poligon)
                {
                    int xi, yi;
                    int xj, yj;
                    int xd = Math.Abs((list[j].Area.X + list[j].Area.Width / 2) - (list[index].Area.X + list[index].Area.Width / 2));
                    int yd = Math.Abs((list[j].Area.Y + list[j].Area.Height / 2) - (list[index].Area.Y + list[index].Area.Height / 2));
                    if (xd > yd)
                    {
                        xi = BinarySearchFunction(yd, xd, list[index].Area.Width / 2, list[index].Area.Height / 2, true);
                        yi = yd * xi / xd;
                    }
                    else
                    {
                        yi = BinarySearchFunction(xd, yd, list[index].Area.Height / 2, list[index].Area.Width / 2, true);
                        xi = xd * yi / yd;
                    }
                    if (xd > yd)
                    {
                        xj = BinarySearchFunction(yd, xd, list[j].Area.Width / 2, list[j].Area.Height / 2, false);
                        yj = yd * xj / xd;
                    }
                    else
                    {
                        yj = BinarySearchFunction(xd, yd, list[j].Area.Height / 2, list[j].Area.Width / 2, false);
                        xj = xd * yj / yd;
                    }
                    int dist = (xi - xj) * (xi - xj) + (yi - yj) * (yi - yj);
                    if (dist < distance || min == -1)
                    {
                        distance = dist;
                        min = j;
                    }
                }
            }
            return min;
        }

        /// <summary>
        /// binary searches through a segment in a 2D positive plane
        /// </summary>
        private static int BinarySearchFunction(int maxResult, int maxVariable, int boundVariable, int boundResult, bool increasing)
        {
            int x, y;
            int low = 0;
            int high = maxVariable;
            if (increasing)
            {
                do
                {
                    x = (low + high) / 2;
                    y = maxResult * x / maxVariable;
                    if (x < boundVariable && y < boundResult)
                    {
                        low = x;
                    }
                    else
                    {
                        high = x;
                    }
                } while (!(x <= boundVariable && y <= boundResult) || (x + 1 < boundVariable && maxResult * (x + 1) / maxVariable < boundResult));
            }
            else
            {
                do
                {
                    x = (low + high) / 2;
                    y = maxResult * x / maxVariable;
                    if (x > maxVariable - boundVariable && y > maxResult - boundResult)
                    {
                        high = x;
                    }
                    else
                    {
                        low = x;
                    }
                } while (!(x >= maxVariable - boundVariable && y >=  maxResult - boundResult) || (x - 1 > maxVariable - boundVariable && maxResult * (x - 1) / maxVariable > maxResult - boundResult));
            }
            return x;

        }
    }
}
