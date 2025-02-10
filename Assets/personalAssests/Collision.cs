/* 
This determines if an object has collided with the object this script is on,
if so it will turn set the object to collision to active
*/
using static System.Math;
using UnityEngine;
 
public class Collision : MonoBehaviour
{
    public ParticleArrayScript particleArrayScript;
   public Vector2 BoundSize;

  public void ResolveCollision(Particle particle)
  {
    Vector2 halfBoundSize = BoundSize /2 - Vector2.one * 0.5f;
    if (Abs(particle.position.x) > halfBoundSize.x)
    {
      particle.velocity.x *= -1;
      particle.position.x = Mathf.Clamp(particle.position.x, -halfBoundSize.x, halfBoundSize.x);
    }
    if (Abs(particle.position.y) > halfBoundSize.y)
    {
      particle.velocity.y *= -1;
      particle.position.y = Mathf.Clamp(particle.position.y, -halfBoundSize.y, halfBoundSize.y);
    }
  }   
    
}