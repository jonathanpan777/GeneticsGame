using UnityEngine;

public class LivingEntity : MonoBehaviour {

    public int colourMaterialIndex;
    public Species species;
    public Material material;

    public MeshRenderer treePrefab;
    public LivingEntity plantPrefab;
    public LivingEntity bunnyPrefab;
    public LivingEntity foxPrefab;

    public float growScale = 0.5f; 


    public Coord coord;
    //
    [HideInInspector]
    public int mapIndex;
    [HideInInspector]
    public Coord mapCoord;

    protected bool dead;

    public virtual void Init (Coord coord) {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];

        // Set material to the instance material
        var meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            if (meshRenderer.sharedMaterials[i] == material) {
                material = meshRenderer.materials[i];
                break;
            }
        }
    }

    public float amountRemaining = 1f;
    public const float consumeSpeed = 8f;
    public float Consume (float amount) {
        float amountConsumed = Mathf.Max (0, Mathf.Min (amountRemaining, amount));
        amountRemaining -= amount * consumeSpeed;

        transform.localScale = Vector3.one * amountRemaining;

        if (amountRemaining <= 0) {
            if (this.species == Species.Plant) {
                Environment.DecrementPlantCount(this.coord, plantPrefab);
            }
            Die (CauseOfDeath.Eaten);
        }

        return amountConsumed;
    }

    public float AmountRemaining {
        get {
            return amountRemaining;
        }
    }

    protected virtual void Die (CauseOfDeath cause) {
        if (!dead) {
            dead = true;
            Environment.RegisterDeath (this);
            Destroy (gameObject);
        }
    }
}