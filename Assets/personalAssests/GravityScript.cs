using UnityEngine;


/*This script is used to update a given particles velocity based on a punlic float value "gravity*/
public class GravityScript : ParticleArrayScript
{
    public float gravity = 9.8f;
    public float dampening = 0.95f;
    public ParticleArrayScript particleArrayScript;
    public void ApplyGravityToParticles(Particle[] particles, float deltaTime) {
        foreach (var particle in particles) {
            particle.velocity += new Vector2(0, -gravity) * deltaTime;
            // gravity dampening
            particle.velocity *= dampening;
            particle.position += particle.velocity * deltaTime;
        }
    }
    public void ApplyGravity(Particle particle) {

            //gravity calculation
            particle.velocity += new Vector2(0, -gravity) * Time.fixedDeltaTime;
            //velocity dampening
            particle.velocity *= dampening;
            particle.position += particle.velocity * Time.fixedDeltaTime;
        
    }
}