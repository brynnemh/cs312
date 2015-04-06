﻿using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TSP
{
    class State : PriorityQueueNode 
	{
		# region static members
		
        private static ArrayList cities;
		
        #endregion

		# region member variables

		private ArrayList tour;
		private double[,] costMatrix;
        public double bound { get; set; }
		
        #endregion 

		#region Constructors

		public State() { }

		/// <summary> creates the start state from a list of cities </summary>
		/// <param name="iRoute"> the list of cities generated by the solver </param>
		public State(ArrayList iRoute)
		{
			cities = iRoute;

			tour = new ArrayList();
			tour.Add(0);
			bound = 0;

			costMatrix = new double[iRoute.Count, iRoute.Count];
			for (int i = 0; i < iRoute.Count; ++i)
			{
				City current = iRoute[i] as City;
				for (int j = i; j < iRoute.Count; ++j)
				{
					double currCost = current.costToGetTo(iRoute[j] as City);
					if (i == j) // on the diagonal
						currCost = double.PositiveInfinity;
					costMatrix[i, j] = currCost;
					costMatrix[j, i] = costMatrix[i, j];
				}
			}
			reduceMatrix();
		}

        /// <summary> creates a deep copy of a state </summary>
        /// <param name="s"> the state to copy (the parent state) </param>
        public State(State s)
        {
            this.tour = new ArrayList(s.tour);
            this.bound = s.bound;

            costMatrix = (double[,])s.costMatrix.Clone();
        }
		
        #endregion

        #region public methods
       
        /// <summary>
        ///  returns all the successors of a state
        ///  adds the next city in the tour based off the previous city
        ///  duplicate cities and impossible cities are eliminated (by the cost matrix)
        /// </summary>
        /// <returns> the list of successors </returns>
        public List<State> successors()
        {
			int lastCity = (int)tour[tour.Count - 1];
			
			List<State> result = new List<State>();
			for (int i = 0; i < cities.Count; ++i)
			{
				State successor = new State(this);
				if (costMatrix[lastCity, i] != Double.PositiveInfinity)
				{
					successor.tour.Add(i);
					successor.bound += costMatrix[lastCity, i];
					successor.reduceMatrix();
					result.Add(successor);
				}
			}
            return result;
        }

        /// <summary> determines if a state is a valid solution </summary>
        /// <returns>
        ///  true if the state is a solution
        ///  false if the state is not a solution
        /// </returns>
		public bool criterion() 
		{
			return tour.Count == cities.Count;		
		}

        /// <summary> returns the list of cities in the order of the tour </summary>
        /// <returns> the tour of the current state </returns>
		public ArrayList getTour()
		{
			ArrayList route = new ArrayList();
			for (int i = 0; i < tour.Count; ++i)
			{
				int city = (int)tour[i];
				route.Add(cities[city]);
			}
			return route;
		}
        
        #endregion

        # region private methods
        
        /// <summary> 
		///  reduces the costMatrix
		///  subtracts the minimum from each row and column and adds it to the bound
		/// </summary>
		private void reduceMatrix() 
		{
			// knock out row and diagonal
			if (tour.Count > 1)
			{
				int secondToLast = (int)tour[tour.Count - 2];
				int last = (int)tour[tour.Count - 1];
				for (int i = 0; i < cities.Count; ++i)
					costMatrix[secondToLast, i] = double.PositiveInfinity;
				for (int i = 0; i < cities.Count; ++i)
					costMatrix[i, last] = double.PositiveInfinity;
			}

			// reduce each row
			for (int i = 0; i < cities.Count; ++i)
			{
				double min = double.PositiveInfinity;
				for (int j = 0; j < cities.Count; ++j)
				{
					if (costMatrix[i, j] < min)
						min = costMatrix[i, j];
				}
				if (min != 0 && min != double.PositiveInfinity)
				{
					for (int j = 0; j < cities.Count; ++j)
					{
						if (costMatrix[i,j] != double.PositiveInfinity) // don't reduce the infinities
							costMatrix[i, j] -= min;
					}
					bound += min;
				}
				
			}
			// reduce each column
			for (int j = 0; j < cities.Count; ++j)
			{
				double min = double.PositiveInfinity;
				for (int i = 0; i < cities.Count; ++i)
				{
					if (costMatrix[i, j] < min)
						min = costMatrix[i, j];
				}
				if (min != 0 && min != double.PositiveInfinity)
				{
					for (int i = 0; i < cities.Count; ++i)
					{
						if (costMatrix[i, j] != double.PositiveInfinity) // don't reduce the infinities
							costMatrix[i, j] -= min;
					}
					bound += min;
				}
				
			}
            // remove impossible edges
			if (tour.Count > 1)
			{
				int lastCity = (int)tour[tour.Count - 1];
		        for (int j = 0; j < tour.Count - 1; ++j)
			    {
				    costMatrix[lastCity, j] = double.PositiveInfinity;
				}
			}
		}
		
        #endregion
	}
}
