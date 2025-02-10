using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class SpatialHashingScript
{
    public ParticleSimulation ParticleSimulation;
    public FluidDynamics FluidDynamics;
    public float cellSize;
    public int tableSize;
    public int[] startTable;
    public int[] denseArray;
    public int[] queryArray;
    public int querySize;
    
    public SpatialHashingScript(float cellSize, int numObjects)
    {
        this.cellSize = cellSize;
        tableSize = 2 * numObjects;
        startTable = new int[tableSize + 1];
        denseArray = new int[numObjects];
        queryArray = new int[numObjects];
        querySize = 0;
    }

    // Optimized hash function for FluidParticle
    public int HashCoordinate(Vector2 coord)
    {
        int x = (int)math.floor(coord.x / cellSize);
        int y = (int)math.floor(coord.y / cellSize);
        int hash = x * 73856093 ^ y * 325613753;
        return math.abs(hash) % tableSize;
    }

    // Hash an array position for FluidParticle
    public int HashArrayPos(FluidDynamics.FluidParticle[] array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }
        return HashCoordinate(array[index].position);
    }

    /*Create
    * for each cell in the grid, the hope  is to correlate it with a respective position in
    * the startTable. by incrementing the startTable for each hashed position, we can then
    * use a partial sum array to derive a dense array to make looking for relative positions 
    * in the initial array easier.
    */
    public void Create(FluidDynamics.FluidParticle[] array)
    {
        //initialize
        int numObjects = array.Length;
        Array.Fill(startTable, 0);
        Array.Fill(denseArray, 0);

        /*
        *Start table
        *for each hashed position increment the startTable
        * this represents how many positions are in that cell
        * conflict: cells may overlap an array position causing a collision
        * solution: Special Hash functions to even out the distribution
        */
        for (int i = 0; i < numObjects; i++)
        {
            var h = HashArrayPos(array, i);
            startTable[h]++;
        }
        /* 
        partial array
        for each cell in the grid, add the previous sum to the current sum
        this will help correlate the position to the index of the dense array
        */
        int sum = 0;
        for (int i = 0; i < tableSize; i++)
        {
            sum += startTable[i];
            startTable[i] = sum;
    
        }
        startTable[tableSize] = numObjects; //gaurd  redundant IF the function works if not... thats why it's here 

        /*dense array
        condense the start array into n size that way we dont need to deal with the empty/ redundant space of the start array
        does this by chronologically looking at the hashed position it was in subtracting 1 and that new value is the position
        in the dense array
        EXAMPLE:
        particle 12 is hashed at startTable[42] which = 37 
        subtract 1 ie 37-1 = 36 
        dense array position is 36: denseArray[36] = 12*/
        for (int i = 0; i < numObjects; i++)
        {
            var h = HashArrayPos(array, i);
            startTable[h]--;
            denseArray[startTable[h]] = i;
        }

    }

    /*Query
    * for a given particle, query the surrounding particles
    * pulls all cells within a given radius.    
    */
    public void Query(FluidDynamics.FluidParticle[] particles, int nr, float radius)
    {   
        querySize = 0;
        /*Because of how this query works with the infinite cell amount collisions happen.
        in order to combat duplicate values I'll deal with the spatial hashing collision with
        genericSystems hashSet that keeping only unique values  */
        HashSet<int> uniqueParticles = new HashSet<int>(); // this is the second place that may be improved upon

        int x0 = (int)math.floor((particles[nr].position.x  - radius)/ cellSize );
        int y0 = (int)math.floor((particles[nr].position.y  - radius)/ cellSize );
        int x1 = (int)math.floor((particles[nr].position.x  + radius)/ cellSize );
        int y1 = (int)math.floor((particles[nr].position.y  + radius)/ cellSize );

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {

                int h = HashCoordinate(new Vector2(x, y));
                for (int i = startTable[h]; i < startTable[h + 1]; i++)
                {
                        if (uniqueParticles.Add(denseArray[i]))
                        {
                        queryArray[querySize] = denseArray[i];
                        querySize++;
                        }   
                }
            }
        } 
     }
}   