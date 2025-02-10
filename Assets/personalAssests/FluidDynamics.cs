using System;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;
using System.Threading.Tasks;


/*This holds all the functions to calculate the pressure 
and apply. as well as calculate density of a particle simulation*/
public class FluidDynamics : MonoBehaviour{

 public ParticleSimulation ParticleSimulation;
 public ParticleArrayScript particleArrayScript;

/*Extend particle classes in order to keep density and pressure dynamicly allocated to the particle*/
public class FluidParticle: Particle
{
    public float density;
    public float pressure;
}
public class FluidParticleArray
{
    //Very similar to the ParticleArrayScript class but with the FluidParticle class
    // also includes the density and pressure properties
    public FluidParticle[] InitializeFluidParticles(int discCount, float discRadius)
    {
        FluidParticle[] particles = new FluidParticle[discCount];
       for (int i = 0; i < discCount; i++) {
            float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            float screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
            float screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

            //Uses system.random to generate random particles positions
            //instead of using Random.Range from UnityEngine
            var random = new System.Random();
            particles[i] = new FluidParticle {
                position = new Vector2((float)(screenLeft + random.NextDouble() * (screenRight - screenLeft)), 
                                       (float)(screenBottom + random.NextDouble() * (screenTop - screenBottom))),
                velocity = Vector2.zero,
                color = Color.HSVToRGB(i / (float)discCount, 1, 1),
                density = 0,
                pressure = 0
            };
        }

        return particles;
    }
 
}
public FluidParticle[] particles;
[Range(0, 10000)] public float mass = 1;
[Range(0, 100)] public float viscosity = 1f;
[Range (0, 100)] public float restDensity = 1;


//set the particles array to the particles array from the ParticleSimulation script
public void shareParticlesinfo()
{
    particles = ParticleSimulation.particles;
}

/*
Smoothing Kernal
This function is used to calculate the influence of a particle
on another particle based on the distance between them
*/

float SmoothingKernal( float radius, float distance)
{   
    float Volume = (float)(PI * Pow(radius , 4)/ 6);
    float kernelvaue =  Max(0, radius- distance);
    return kernelvaue * kernelvaue / Volume;
}

/*
This function helps visualize the density by returning 
the influence(amount of particles that are close and how close)
and multiplying it by the mass of the particle pulled from the ParticleSimulation script
*/

public float densityIndParCalculation(Particle particle)
{
    float density = 0;
    foreach (var otherParticle in particles)
    {   

        float distance = Vector2.Distance(particle.position, otherParticle.position);
        float influence = SmoothingKernal(ParticleSimulation.particleInfluenceRadius, distance);
        density += influence * mass;
    }
    
    return density;
}
public float densityIndParCalculation(Particle particle, int[] IDs) {
    float density = 0;
    HashSet<int> uniqueIDs = new HashSet<int>();
    foreach(int id in IDs) {
        //Checks//////////////
            // Check for out of bound ID, end of list
            if (id == particles.Length+1) {
                Debug.LogWarning("ID is larger than the particle array length");
                break;
            }

            // Check for duplicate IDs
            if (!uniqueIDs.Add(id)) {
                Debug.LogWarning("Duplicate ID found: " + id); 
                continue;
            }
        //Density Calculation//////
        FluidParticle otherParticle = particles[id];
        float distance = Vector2.Distance(particle.position, otherParticle.position);
        float influence = SmoothingKernal(ParticleSimulation.particleInfluenceRadius, distance);
        density += influence * mass;
        }
    // Final density value
    return density;
}

/*This function is used to calculate the pressure of a given particle by taking
 its density and returning a pressure*/
    public float pressureCalculation(float density){
        return 0.5f * (density - restDensity); 
    }

/* This function is used to update the velocity of a 
particle given the particle and pressure*/

public void ApplyPressure(FluidParticle particle){
    if (particle == null){ 
        Debug.Log("particle is null");  
        return;
    }
    if (particles == null){
        Debug.Log("particles array is null");
        return;
    }
    var random = new System.Random();  // Thread-local random instance
    Parallel.ForEach(particles, otherParticle =>
    {  
        float distance = Vector2.Distance(particle.position, otherParticle.position);
        float influence = SmoothingKernal(ParticleSimulation.particleInfluenceRadius, distance);
        float pressure = pressureCalculation(particle.density);
        // if particle positions are the same pick a random direction
        // make random thread safe by using a thread-local random instance
        
        Vector2 direction = (particle.position == otherParticle.position)
            ? new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1).normalized
            : (particle.position - otherParticle.position).normalized;
        // viscosity calculation
        float viscosity = 1f;
        // convective acceleration: 
        particle.velocity += otherParticle.velocity * influence * 1/ParticleSimulation.particleCount;
        // both particles should feel the same force in opposite directions
        //calc velocity by adding mass(pressure/density^2)*influence*direction*viscosity iteratively through particles
        particle.velocity += mass * pressure / (particle.density * particle.density)* influence * direction * viscosity;
        otherParticle.velocity -= mass * pressure / (particle.density * particle.density) * influence * direction * viscosity;
        // Direction pressure influence and velocity are printed to the console
        //Debug.Log("density: " + particle.density + " pressure: " + pressure + " influence: " + influence + " Particle velocity: " + particle.velocity + " , " + otherParticle.velocity);
    });
}
public void ApplyPressure(FluidParticle particle , int[] IDs){   
    if (particle == null){ 
        Debug.Log("particle is null");  
        return;
    }
    if (particles == null){
        Debug.Log("particles array is null");
        return;
    }
    var random = new System.Random();  // Thread-local random instance
    HashSet<int> uniqueIDs = new HashSet<int>();
    foreach(int id in IDs) {
        //Checks//////////////
            // Check for ou of bound ID, end of list
            if (id == particles.Length+1) {
                //Debug.LogWarning("ID is null");
                break;
            }

            // Check for duplicate IDs
            if (!uniqueIDs.Add(id)) {
                Debug.LogWarning("Duplicate ID found: " + id); 
                break;
            }
        //Pressure Calculation//////
        FluidParticle otherParticle = particles[id];
        float distance = Vector2.Distance(particle.position, otherParticle.position);
        float influence = SmoothingKernal(ParticleSimulation.particleInfluenceRadius, distance);
        float pressure = pressureCalculation(particle.density);
        // if particle positions are the same pick a random direction
        // make random thread safe by using a thread-local random instance
        
        Vector2 direction = (particle.position == otherParticle.position)
            ? new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1).normalized
            : (particle.position - otherParticle.position).normalized;
        // viscosity calculation
        // convective acceleration: 
        particle.velocity += otherParticle.velocity * influence * 1/ParticleSimulation.particleCount;
        // both particles should feel the same force in opposite directions
        //calc velocity by adding mass(pressure/density)*influence*direction*viscosity iteratively through particles
        particle.velocity += mass * pressure / particle.density * influence * direction * viscosity;
        otherParticle.velocity -= mass * pressure / particle.density  * influence * direction * viscosity;
        // Direction pressure influence and velocity are printed to the console
        //Debug.Log("density: " + particle.density + " pressure: " + pressure + " influence: " + influence + " Particle velocity: " + particle.velocity + " , " + otherParticle.velocity);
    }
}

}
