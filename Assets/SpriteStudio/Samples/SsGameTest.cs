/**
	SpriteStudio
	
	Button animation sample
	
	The reason why the almost member fields are public is to show in inspector to observe their state.
	
	Copyright(C) 2003-2011 Web Technology Corp. 
	All rights reserved
*/

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SsSprite))]
public class SpriteObject : MonoBehaviour
{
	static	public	SsAssetDatabase	_ssDatabase;
	
	protected	delegate void UpdateFunc();
	protected	UpdateFunc	_updateFunc;
	
	Transform	_transform;

	public	SsSprite	_sprite;
	
	// Position accessor
	public	Vector3		_pos {
		get {return _transform.position;} 
		set {_transform.position = value;}
	}
	public	float		_xpos {
		get {return _pos.x;}
		set {
			Vector3 v = _pos;
			v.x = value;
			_pos = v;
		}
	}
	public	float		_ypos {
		get {return _pos.y;}
		set {
			Vector3 v = _pos;
			v.y = value;
			_pos = v;
		}
	}
	public	float		_zpos {
		get {return _pos.z;}
		set {
			Vector3 v = _pos;
			v.z = value;
			_pos = v;
		}
	}
	// Rotation accessor
	public	Vector3		_rot {
		get {return _transform.localRotation.eulerAngles;} 
		set {
			Quaternion rot = Quaternion.identity;
			rot.eulerAngles = value;
			_transform.localRotation = rot;
		}
	}
	public	float		_xrot {
		get {return _rot.x;}
		set {
			Vector3 v = _rot;
			v.x = value;
			_rot = v;
		}
	}
	public	float		_yrot {
		get {return _rot.y;}
		set {
			Vector3 v = _rot;
			v.y = value;
			_rot = v;
		}
	}
	public	float		_zrot {
		get {return _rot.z;}
		set {
			Vector3 v = _rot;
			v.z = value;
			_rot = v;
		}
	}
	// Scale accessor
	public	Vector3		_scl {
		get	{return _transform.localScale;}
		set	{_transform.localScale = value;}
	}
	public	float		_xscl {
		get {return _scl.x;}
		set {
			Vector3 v = _scl;
			v.x = value;
			_scl = v;
		}
	}
	public	float		_yscl {
		get {return _scl.y;}
		set {
			Vector3 v = _scl;
			v.y = value;
			_scl = v;
		}
	}
	public	float		_zscl {
		get {return _scl.z;}
		set {
			Vector3 v = _scl;
			v.z = value;
			_scl = v;
		}
	}

	void Awake()
	{
		_transform = transform;
		_sprite = GetComponent<SsSprite>();
		_sprite.DrawBoundingBox = SsGameTest._instance._boundingBox;
	}

	public virtual void Update()
	{
		if (_updateFunc != null)
			_updateFunc();
	}

	public bool	EqulasAnime(string name)
	{
		return _sprite.Animation && _sprite.Animation.name == name;
	}
		
	public virtual bool ChangeAnime(string name)
	{
 		if (EqulasAnime(name)) return false;
		_sprite.Animation = _ssDatabase.GetAnime(name);
		_sprite.Play();
		return true;
	}
}

public class PlayerObject : SpriteObject
{
	public		float			_moveSpeed = 10f;
	public		LongStickObject	_swordObject;
	float		_orgSwordLength;
	float		_orgSwordWidth;

	void Start()
	{
		// attach a long stck as sword in my hand
		GameObject swordGo = new GameObject("sword");
		_swordObject = swordGo.AddComponent<LongStickObject>();
		_swordObject.Init();

		ChangeAnime("stand_ssa");
		_updateFunc = _Idling;
	}
	public override bool ChangeAnime(string name)
	{
		if (EqulasAnime(name)) return false;

		// due to lose children transform while changing animation, detach temporarily
		Transform	swordTransform = _swordObject._sprite.TransformAt(null);
		swordTransform.parent = null;

		base.ChangeAnime(name);
		
		// create and get the transform of my hand.
		SsPart handPart = _sprite.GetPart("前手");
		Transform	handTransform = handPart.CreateTransform();
#if false // obsolete since version 1.06
		// reset sword position
		swordTransform.localPosition = Vector3.zero;
		swordTransform.localRotation = Quaternion.identity;
		swordTransform.localScale = Vector3.one;
		// attach
		swordTransform.parent = handTransform;
#else
		// attach
		swordTransform.parent = handTransform;
		// reset sword position
		swordTransform.localPosition = Vector3.zero;
		swordTransform.localRotation = Quaternion.identity;
		swordTransform.localScale = Vector3.one;
#endif
		// move a bit over the hand
		_swordObject._zpos -= 0.01f;

#if false // obsolete since version 1.06
		// due to sword flashes at root positon, give it correct position at first frame.
		_sprite.UpdateAlways();
#endif
		return true;
	}

	void _Idling()
	{
		_sprite.PlayCount = 0;
		if (Input.GetKey(KeyCode.UpArrow))
		{
			ChangeAnime("jump_ssa");
			_sprite.AnimationFinished = JumpFinished;
			_sprite.PlayCount = 1;
#if false
			Vector3 pos = _pos;
			pos.y += 800;
			Fire(pos, 2);
#endif
			_updateFunc = _Jumping;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			_sprite.hFlip = false;
			ChangeAnime("dash_ssa");
			_xpos -= _moveSpeed;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			_sprite.hFlip = true;
			ChangeAnime("dash_ssa");
			_xpos += _moveSpeed;
		}
		else
		{
			ChangeAnime("stand_ssa");

			if (Input.GetButtonDown("Fire1"))
			{
				Swing();
			}
			if (Input.GetButtonDown("Fire2"))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Fire(ray.GetPoint(0f), 1);
			}
			if (Input.GetButtonDown("Fire3"))
			{
				GameObject bombGo = new GameObject("bomb");
				bombGo.AddComponent<BombObject>();
			}
		}
	}
	
	void _Jumping()
	{
	}
		
	public void JumpFinished(SsSprite sprite)
	{
		_updateFunc = _Idling;
		ChangeAnime("stand_ssa");
	}

	public void Fire(Vector3 targetPos, float scale)
	{
		GameObject go = new GameObject("stick");
		StickObject so = go.AddComponent<StickObject>();
		so._scl *= scale;
		if (scale > 1)
		{
			so. _beChameleon = true;
			so._pos = _sprite.PositionAt("リボン");
		}
		else
		{
			so._pos = _sprite.PositionAt("前手");
		}
		so._zpos = -0.1f;
		so._sprite.hFlip = _sprite.hFlip;
		so.Init(targetPos);
	}
	
	public void Swing()
	{
		ChangeAnime("attack_ssa");
		// set animation finished callback to turn off the collision detection
		_sprite.AnimationFinished = SwingFinished;
		_sprite.PlayCount = 1;
		// get a part to use as the trigger to start collision detection 
		SsPart part = _sprite.GetPart("基点");
		// set the callbacks of sound/user key.
		part.OnSoundKey += OnSoundKey;
		part.OnUserDataKey += OnUserDataKey;
		// save current sword length
		_orgSwordLength = SsGameTest._instance._swordLength;
		_orgSwordWidth = SsGameTest._instance._swordWidth;
		// change current state
		_updateFunc = _Swinging;
	}
	
	void _Swinging()
	{
		// nothing to do...
	}

	public void OnSoundKey(SsPart part, SsAttrValueInterface val)
	{
		SsSoundKeyValue sv = val as SsSoundKeyValue;
		Debug.LogWarning(part.Sprite.gameObject.name + ": " + sv.SoundId);
		part.OnSoundKey -= OnSoundKey;
	}

	public void OnUserDataKey(SsPart part, SsAttrValueInterface val)
	{
		SsUserDataKeyValue u = val as SsUserDataKeyValue;
		if (u.String == "HIT!!!")
		{
			// start collision detection
			_swordObject.EnableCollision(true);
			SsGameTest._instance._swordLength *= 1.5f;
			SsGameTest._instance._swordWidth *= 1.25f;
		}
		if (u.String == "DONE")
		{
			// restore the sword length
			SsGameTest._instance._swordLength = _orgSwordLength;
			SsGameTest._instance._swordWidth = _orgSwordWidth;
			part.OnUserDataKey -= OnUserDataKey;
		}
	}	

	public void SwingFinished(SsSprite sprite)
	{
		// restore the sword length surely
		SsGameTest._instance._swordLength = _orgSwordLength;
		SsGameTest._instance._swordWidth = _orgSwordWidth;
		// end collision detection
		_swordObject.EnableCollision(false);
		_updateFunc = _Idling;
		ChangeAnime("stand_ssa");
	}
}

public class BeeObject : SpriteObject
{
	public	GameObject	_player;
	public	float		_timer;
	public	Vector3		_velocity;
	public	float		_wafture;
	public	float		_waftSpeed;
	public	bool		_toTarget;
	SsPart				_bodyPart;

//	void Awake()
//	{
//		tag = "bee";
//	}

	void Start()
	{
		_xpos = Random.Range(-200, 200);
		_ypos = Random.Range(500, 700);
		_zpos = -0.01f;
		_rot = Vector3.zero;
		Update();
		ChangeAnime("bee_ssa");
		_bodyPart = _sprite.GetPart("体");
		Reset();
		_sprite.Play();
		_sprite.AnimFrame = Random.Range(0, 3);
	}
	
	void Reset()
	{
		_timer = SsGameTest._instance._attackTime + Random.Range(-1, +1);
		_toTarget = Random.Range(0, 100) <= SsGameTest._instance._attackRate;

		if (_toTarget && _player)
		{
			// pursue to the player
			SsSprite playerSprite = _player.GetComponent<SsSprite>();
			Vector3 targetPos = playerSprite.Position;
			targetPos.y += 450;
			targetPos.x += Random.Range(-80, +80);
			_velocity = (targetPos - _pos) / (_timer / Time.deltaTime);
			_velocity.z = 0f;
			// go red
			_bodyPart.IndividualizeMaterial(true);
			_bodyPart.GetMaterial().color = Color.red;
		}
		else
		{
			// random flying
			_velocity = new Vector3(Random.Range(-2f, 2f) / 4, Random.Range(-2f, 2f) / 4, 0f);
			// calm down
			_bodyPart.RevertChangedMaterial();
		}
		_wafture = Random.Range(1f, 2f);
		_waftSpeed = Random.Range(4f, 8f);
		_zrot = 0f;
		_sprite.vFlip = false;
		_updateFunc = _Idling;
	}
	
	void _Idling()
	{
		_sprite.hFlip = _velocity.x > 0;
		_pos += _velocity;
		_ypos += Mathf.Sin(Mathf.Deg2Rad * (Time.frameCount * _waftSpeed  % 360)) * _wafture;
		_timer -= Time.deltaTime;
		if (_timer <= 0f)
		{
			Reset();
		}
	}
	
	public void OnHit()
	{
		_bodyPart.RevertChangedMaterial();

		_sprite.vFlip = true;
		_sprite.Pause();
		_updateFunc = _Falling;
		
		// engender bomb particle
		UnityEngine.Object.Instantiate(SsGameTest._instance._bombParticle, _pos, Quaternion.identity);
	}
	
	void _Falling()
	{
		_velocity.y -= 0.2f;
		_pos += _velocity;
		_zrot += 12;
	}
	
	public bool IsFalling()
	{
		return _updateFunc == _Falling;
	}

	void OnBecameInvisible()
	{
		if (_updateFunc == _Falling)
			Start();
	}
}

public class StickObject : SpriteObject
{
	public	Vector3		_velocity;
	public	bool		_beChameleon;
	
	public void Init(Vector2 target)
	{
		_velocity.x = target.x - _pos.x;
		_velocity.y = (target.y + 80) - _pos.y;
		_velocity /= 30;
	}
	
	void Start()
	{
		Update();
		ChangeAnime(_beChameleon ? "chameleon_ssa" : "stick_ssa");
		_updateFunc = _Idling;
	}
	
	void _Idling()
	{
		_velocity.y -= 0.5f;
		_pos += _velocity;
		
		// check hitting all bees on the scene
		//GameObject[] bees = GameObject.FindGameObjectsWithTag("bee");
		foreach (GameObject e in SsGameTest._instance._bees)
		{
			BeeObject bo = e.GetComponent<BeeObject>();
			if (bo.IsFalling()) continue;
			if (bo._sprite.IntersectsByBounds(_sprite, true))
			{
				bo.OnHit();
				if (!SsGameTest._instance._penetratableFire && !_beChameleon)
					Object.Destroy(gameObject);
				break;
			}
		}
	}

	void OnBecameInvisible()
	{
		Object.Destroy(gameObject);
	}
}

public class LongStickObject : SpriteObject
{
	public void Init()
	{
		ChangeAnime("longstick_ssa");
	}
	
	public override void Update()
	{
		base.Update();
		_xscl = SsGameTest._instance._swordLength;
		_yscl = SsGameTest._instance._swordWidth;
	}

	public void EnableCollision(bool enable)
	{
		if (enable)
			_updateFunc = _Swinging;
		else
			_updateFunc = null;
	}
	
	void _Swinging()
	{
		// check hitting all bees on the scene
		//GameObject[] bees = GameObject.FindGameObjectsWithTag("bee");
		foreach (GameObject e in SsGameTest._instance._bees)
		{
			BeeObject bo = e.GetComponent<BeeObject>();
			if (bo.IsFalling()) continue;
			if (bo._sprite.IntersectsByBounds(_sprite, true))
			{
				bo.OnHit();
			}
		}
	}
}

public class BombObject : SpriteObject
{
	void Start()
	{
		int efc_num = Random.Range(1, 7);
		string ssa_name = System.String.Format("effect{0:000}_ssa", efc_num);
		ChangeAnime(ssa_name);
		_sprite.PlayCount = 1;
		_sprite.AnimationFinished = AnimeFinished;
		_ypos = efc_num == 6 ? 800.0f : 500.0f;
		_zpos = -0.2f;
	}
	
	void AnimeFinished(SsSprite sprite)
	{
		// all bees in visible now are fell out of the scene.
		foreach (GameObject e in SsGameTest._instance._bees)
		{
			BeeObject bo = e.GetComponent<BeeObject>();
			if (bo.IsFalling()) continue;
			bo.OnHit();
		}
		// vaporize
		Object.Destroy(gameObject);
	}
}

public class SsGameTest : MonoBehaviour
{
	static	public	SsGameTest	_instance;
	
	public	SsAssetDatabase		_ssDatabase;
	public	GameObject			_bombParticle;
	public	GameObject			_player;
	public	GameObject[]		_bees;
	//--------- adjustable parameters
	public	int					_attackRate = 30;
	public	int					_attackTime = 1;
	public	bool				_penetratableFire;
	public	int					_numOfBees = 8;
	public	float				_swordLength = 1.25f;
	public	float				_swordWidth = 1f;
	public	bool				_boundingBox = false;

	public SsGameTest()
	{
		_instance = this;
	}

	void Awake()
	{
		// enable access to database inside each sprite object.
		SpriteObject._ssDatabase = _ssDatabase;

		_player = new GameObject("player");
		_player.AddComponent<PlayerObject>();

		_bees = new GameObject[_numOfBees];
		for (int i = 0; i < _bees.Length; ++i)
		{
			_bees[i] = new GameObject("bee");
			BeeObject bo = _bees[i].AddComponent<BeeObject>();
			bo._player = _player;
		}
	}

	void Update()
	{
	}
}
