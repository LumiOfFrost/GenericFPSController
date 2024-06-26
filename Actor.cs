using Godot;
using System;
using System.Linq.Expressions;

public partial class Actor : CharacterBody3D
{
	
	public float health = 5;
    [Export]
    public float maxHealth = 5;
    public bool isDead = false;

	public float gravity = -ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready() {

        health = maxHealth;

    }

    public virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal, Node3D source) 
    {

        health -= damage;
        if (health <= 0 && !isDead) 
        {
            OnDeath();
        }

    }

    public virtual void OnDeath() 
    {

        isDead = true;

        //This is where Death code would go.
        //IF I HAD SOME.

    }
    
}
