using UnityEngine;

public class ParticleArrayScript : MonoBehaviour {

    public Particle[] InitializeParticles(int discCount, float discRadius) { //disc == particle
        Particle[] particles = new Particle[discCount];

        for (int i = 0; i < discCount; i++) {
            float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            float screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
            float screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

            particles[i] = new Particle {
                position = new Vector2(Random.Range(screenLeft, screenRight), Random.Range(screenBottom, screenTop)),
                velocity = Vector2.zero,
                color = Color.HSVToRGB(i / (float)discCount, 1, 1)
            };
        }

        return particles;
    }
    //function that calculates the particles position based on velocity
    
}
public class Particle {
    public Vector2 position;
    public Vector2 velocity;
    public Color color;
}