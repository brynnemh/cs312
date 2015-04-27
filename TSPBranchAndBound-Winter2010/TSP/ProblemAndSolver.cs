using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace TSP
{
    class ProblemAndSolver
    {
        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// you are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
                //State initial = new State(iroute);
            }

            /// <summary>
            ///  compute the cost of the current route.  does not check that the route is complete, btw.
            /// assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }
                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }

            public String ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('[');
                foreach (City c in Route)
                {
                    builder.Append("[" + c.X + "," + c.Y + "],");
                }
                builder.Remove(builder.Length - 1, 1); //Remove extra comma at end
                builder.Append(']');

                return builder.ToString();

            }

        }

        #region private members
        private const int DEFAULT_SIZE = 25;
        
        private const int CITY_ICON_SIZE = 5;

        private TimeSpan timeLimit = new TimeSpan(0,1,0);//,0,0,0);

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        
		/// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        
		/// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region public members.
        public int Size
        {
            get { return _size; }
        }

        //Possibly put the agenda here

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        public const int DEFAULT_SEED = -1;

        #region Constructors
        public ProblemAndSolver()
        {
            initialize(DEFAULT_SEED, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed)
        {
            initialize(seed, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed, int size)
        {
            initialize(seed, size);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// reset the problem instance. 
        /// </summary>
        private void resetData()
        {
            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null; 

            for (int i = 0; i < _size; i++)
                Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.LightGray,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        private void initialize(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            if (seed != DEFAULT_SEED)
                this.rnd = new Random(seed);
            else
                this.rnd = new Random();
            this.resetData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size)
        {
            this._size = size;
            resetData(); 
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-15F;
            Font labelFont = new Font("Arial", 10);

            g.DrawString("n(c) means this node is the nth node in the current solution and incurs cost c to travel to the next node.", labelFont, cityBrushStartStyle, new PointF(0F, 0F)); 

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        ///  right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            Route = new ArrayList();
            Stopwatch timer = new Stopwatch();
            // baseline
            //timer.Start();
            //bssf = new TSPSolution(baseLine());
            //timer.Stop();

            Agenda agenda = new Agenda();

            //bssf	
            ArrayList sol = quickSolution();
            bssf = new TSPSolution(sol);
            double bssfCost = costOfBssf();
            //init_state
            State initial = new State(sol);

            agenda.add(initial);

            timer.Start();
            int depth = 1; // tour starts at 1 city
            while (!agenda.empty() && timer.Elapsed < timeLimit && bssfCost != agenda.first(depth).bound)
            {
                State s = agenda.first(depth); //initial; // change to agenda.first()
                agenda.remove_first(depth);
                if (s.bound < bssfCost)
                {
                    List<State> children = s.successors();
                    foreach (State child in children)
                    {
                        if (!(timer.Elapsed < timeLimit))
                        {
                            timer.Stop();
                            break;
                        }
                        if (child.bound < bssfCost)
                        {
                            if (child.criterion())
                            {
                                Console.Write("updated BSSF: " + costOfBssf());
                                bssf = new TSPSolution(child.getTour());
                                bssfCost = costOfBssf();
                            }
                            else
                                agenda.add(child);
                        }
                    }
                }
                depth++;
            }
            timer.Stop();

            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.costOfRoute();
            // print out the time elapsed
            Program.MainForm.tbElapsedTime.Text = timer.Elapsed.ToString();
            // do a refresh. 
            Program.MainForm.Invalidate();

            // print to clipboard
            Clipboard.SetText(bssf.ToString());
            // System.Console.Write(bssf.ToString());
        }
        #endregion

        public ArrayList quickSolution()
        {
            ArrayList quick = new ArrayList();

            City first = Cities[0];
            double champ = 0;
            City other = null;

            for (int x = 1; x < Cities.Length; x++)
            {
                double dist = first.costToGetTo(Cities[x]);
                if(dist > champ)
                {
                    champ = dist;
                    other = Cities[x];
                }
            }

            quick.Add(Cities[0]);
            quick.Add(other);

            while (quick.Count != Cities.Length)
            {
                City insert = null;
                double winner = 0;

                for (int i = 1; i < Cities.Length; i++)
                {
                    City r = Cities[i];
                    if (!quick.Contains(r))
                    {
                        double loser = Double.PositiveInfinity;
                        for (int j = 0; j < quick.Count; j++)
                        {
                            double path = r.costToGetTo((City)quick[j]);
                            if (path < loser)
                            {
                                loser = path;
                            }
                        }
                        if (winner < loser)
                        {
                            winner = loser;
                            insert = r;
                        }
                    }
                }

                double minIntersect = Double.PositiveInfinity;
                int frontindex = -1;
                int behindindex = -1;

                for (int k=0; k < quick.Count; k++)
                {
                    City begin = (City)quick[k % quick.Count];
                    City last = (City)quick[(k + 1) % quick.Count];
                    double cir = begin.costToGetTo(insert);
                    double crj = insert.costToGetTo(last);
                    double cij = begin.costToGetTo(last);
                    double total = cir + crj - cij;
                    if(total < minIntersect)
                    {
                        minIntersect = total;
                        frontindex = k;
                        behindindex = k + 1;
                    }
                }
                quick.Insert(behindindex, insert);
            }
            return quick;
        }

        public ArrayList baseLine()
        {
            // greedy algorithm
            // create cost matrix
            double[,] costMatrix = new double[Cities.Length,Cities.Length];
            for (int i = 0; i < Cities.Length; ++i)
            {
                City current = Cities[i];
                for (int j = i; j < Cities.Length; ++j)
                {
                    double currCost = current.costToGetTo(Cities[j]);
                    if (i == j) // on the diagonal
                        currCost = double.PositiveInfinity;
                    costMatrix[i, j] = currCost;
                    costMatrix[j, i] = costMatrix[i, j];
                }
            }

            ArrayList greedySolution = new ArrayList();
            City first = Cities[0];
            greedySolution.Add(first); // add the first city
            City previous = first;
            int index = 0;
            do
            {
                // mark out previous city in costMatrix 
                for (int i = 0; i < Cities.Length; ++i)
                    costMatrix[i, index] = double.MaxValue;

                // find the min distance in the row
                double min = double.MaxValue;
                int winner = -1;
                for (int i = 0; i < Cities.Length; ++i)
                {
                    if (costMatrix[index,i] < min)
                    {
                        min = costMatrix[index, i];
                        winner = i;
                    }
                }
                City nextInRoute = Cities[winner];
                greedySolution.Add(nextInRoute);

                // update index
                index = winner;
                previous = Cities[winner];
            }
            while (greedySolution.Count != Cities.Length);

            return greedySolution;
        }
    }
}
