using UnityEngine;
using Shapes;
using static FluidDynamics;
using System;


public class ParticleSimulation : ImmediateModeShapeDrawer {
    [Range(0, 10000)] public int particleCount = 24;
    [Range(0.01f, 1f)] public float particleRadius = 0.1f;
    [Range(0.01f, 10f)] public float particleInfluenceRadius = 1f;
    public bool colorParVelDen = false;
    public bool hash = false;
    public ParticleArrayScript particleArrayScript; // Reference to ParticleArrayScript
    public GravityScript GravityScript; // Reference to GravityScript
    public Collision CollisionScript;// Reference to CollisionScript
    public FluidDynamics fluidDynamics; // Reference to FluidDynamics
    public SpatialHashingScript spatialHashingScript; // Reference to SpatialHashingScript
    //public SimulationControls simulationControls; // Reference to SimulationControls
    public FluidParticle[] particles;// Array to hold initialized particles
    SimulationControls controls = new SimulationControls();
    FluidParticleArray fluidParticleArray = new FluidParticleArray();
    
    public override void DrawShapes(Camera cam) {
        SpatialHashingScript Hashing = new SpatialHashingScript(1f, particleCount); // may be redundant
        using (Draw.Command(cam)) {
            Draw.ResetAllDrawStates();
            Draw.Matrix = transform.localToWorldMatrix;
            
                // Initialize the particle array using the ParticleArrayScript
                // if null or prees keyboard 'r' to reset the particles
                if (particles == null || Input.GetKeyDown(KeyCode.R)) {
                    particles = fluidParticleArray.InitializeFluidParticles(particleCount, particleRadius);
                fluidDynamics.shareParticlesinfo();
                }

                // Create spatial hashing
                Hashing.Create(particles);
                

                for(int i = 0; i < particles.Length; i++){
                    FluidParticle particle = particles[i];
                
                //query the particles//
                    Hashing.Query(particles, i , particleInfluenceRadius);
                    int[] queryArray = new int[Hashing.querySize];
                    Array.Copy(Hashing.queryArray, queryArray, Hashing.querySize);
                
                //Update particles Density //
                    if (hash){particle.density = fluidDynamics.densityIndParCalculation(particle, queryArray);}else{ particle.density = fluidDynamics.densityIndParCalculation(particle); }
                    // Apply fluid dynamics to particles
                    if (hash){ fluidDynamics.ApplyPressure(particle, queryArray);} else{fluidDynamics.ApplyPressure(particle);}
                    // Debug: check if any particle has a null value in density velocity or position
                    if (particle.density == 0 || particle.velocity == null || particle.position == null){
                        Debug.LogWarning("Particle has a null value in density, velocity or position");}
                }
                // Draw particles
                foreach (var particle in particles) {
                    // Apply gravity to particles
                    GravityScript.ApplyGravity(particle);
                    //Apply collision to particles
                    CollisionScript.ResolveCollision(particle);
                    // Update particle color based on density or velocity
                    if (colorParVelDen){
                        particle.color = Color.HSVToRGB(0, 0, particle.velocity.magnitude);
                    }
                    else{
                    particle.color = Color.HSVToRGB(.5f, 1f, particle.density/10);}
                    // Draw particles
                    Draw.Disc(particle.position, particleRadius, particle.color);
                }
                
                controls.controls();
        }
    }
}