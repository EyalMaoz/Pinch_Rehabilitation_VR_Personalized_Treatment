using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketsTriggers : MonoBehaviour
{
    public enum BucketType
    {
        Yellow, Green, Red,Blue
    }
    public BucketType bucketColor;
    public ParticleSystem Confety;

    private void OnTriggerEnter(Collider other)
    {
        Color color;
        switch (bucketColor)
        {
            case BucketType.Red: color = Color.red; break;
            case BucketType.Green: color = Color.green; break;
            case BucketType.Yellow: color = Color.yellow; break;
            case BucketType.Blue: color = Color.blue; break;
            default: color = Color.red; break;
        }
        if (other.gameObject.GetComponent<Renderer>().material.color == color)
        {
            Confety.Play(true);
            GameObject.Destroy(other.gameObject);
        }
    }
}
