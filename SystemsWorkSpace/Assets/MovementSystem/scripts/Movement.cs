using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Movement : MonoBehaviour
{
	[Header("General")]
	[SerializeField] private GameObject objectWithModel;
	
	[Header("Camera Settings")]
	[SerializeField] private Transform cameraPosition;
	[SerializeField] private Vector2   sensativity =  new Vector2(300, 300);
	[SerializeField] private Vector2   maxLookAngle = new Vector2(-90, 90);
	
	[Header("Ground Check")]
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float     groundDrag =    5;
	[SerializeField] private bool      useCayotyTime = false;
	[SerializeField] private float     cayotyTime =    0.4f;
	
	[Header("Speeds")]
	[SerializeField] private float   walkSpeed = 5;
	[SerializeField] private bool    lowerSpeedInAir = false;
	[SerializeField] private bool    upperSpeedInAir = true;
	[SerializeField] private bool    noInputNoSpeedGround = false;
	[SerializeField] private bool    noInputNoSpeedAir = true;
	[SerializeField] private bool    smoothSpeed = false;
	[SerializeField] private Vector2 smoothSpeedAccelerationGround = new Vector2(10, 10); // lowering / upering
	[SerializeField] private Vector2 smoothSpeedAccelerationAir = new Vector2(10, 10); // lowering / upering
	[SerializeField] private bool    drasticSmoothSpeed = false;
	[SerializeField] private Vector2 drastic = new Vector2(10, 10); // lowering / upering
	
	[Header("Gravitation")]
	[SerializeField] private float normalGravitation =  9.81f;
	[SerializeField] private float fallingGravitation = 9.81f;
	
	[Header("Jump")]
	[SerializeField] private bool  jump;
	[SerializeField] private bool  holdJumpButton;
	[SerializeField] private float jumpHeight;
	[SerializeField] private float jumpRememberTime;
	[SerializeField] private float jumpDelay;
	//jump cut
	[SerializeField] private bool  jumpCut;
	[SerializeField] private float jumpCutStrength;
	//dubble jump
	[SerializeField] private bool  dubbleJump;
	[SerializeField] private int   ammountOfJumpsInAir;
	[SerializeField] private float dubbleJumpHeight;
	[SerializeField] private float dubbleJumpDelay;
	//jump speed manipulations
	[SerializeField] private bool  jumpSpeedManipulations;
	[SerializeField] private float increaseSpeedAfterJumpBy;
	[SerializeField] private float maxObtainableSpeedByJump;
	
	[Header("Input")]
	[SerializeField] private KeyCode jumpKey;
	[SerializeField] private KeyCode sprintKey;
	[SerializeField] private KeyCode crouchKey;
	[SerializeField] private KeyCode slideKey;
	[SerializeField] private KeyCode wallRunUpKey;
	[SerializeField] private KeyCode wallRunDownKey;
	
	[Header("Debug Variables")] //sets automaticaly
	[SerializeField] private bool      debug = false;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Transform orientation;
	[SerializeField] private Transform cam;
	[SerializeField] private Transform cameraHolder;
	[SerializeField] private Vector3   groundCheckSize;
	[SerializeField] private float     groundCheckLength;
	
	[Header("Debug Text")]
	[SerializeField] private bool       debugText = false;
	[SerializeField] private GameObject debugTextPrefab;
	
	[Header("Gizmos")]
	[SerializeField] private bool gizmosAllways;
	[SerializeField] private bool gizmosSelected;
	[SerializeField] private bool groundCheck;
	
	//privates (no need to modify at all)
	//state
	private enum movementTypes {stand, air, walking, sprinting, crouching};
	private movementTypes moveType;
	//camera
	private float cameraRotationX;
	private float cameraRotationY;
	private float cameraRotationZ;
	//groundcheck
	private bool  realGrounded;
	private bool  grounded;
	private float cayotyTimer;
	//speed
	Vector3 moveDirection;
	private float realSpeed;
	private float desiredSpeed;
	private float lastDesiredSpeed;
	private float lastLastDesiredSpeed;
	private float deltaSpeedCounter;
	private float deltaSpeedCounterSpeed;
	//jump
	private int   jumpAmm;
	private float jumpRememberTimer;
	private float jumpDelayTimer;
	private float jumpedTimer;
	//debug text
	private TextMeshProUGUI realSpeedText;
	private TextMeshProUGUI desiredSpeedText;
	private TextMeshProUGUI rigidbodySpeedText;
	private TextMeshProUGUI stateText;
	//bools to control state
	private bool noInput;
	//Gravity changing
	private Vector3 currUp;
	private float gravitation;
	
	private void Start()
	{
		/*all the variables here are seting automaticaly*/
		
		//disable mouse when in player
		MouseDisable();

		//get renderer for later use
		Renderer rend = objectWithModel.transform.GetComponent<Renderer>();
		
		//set start up direction
		currUp = transform.up;
		
		//Rigidbody
		if (rb == null && gameObject.GetComponent<Rigidbody>() == null)
			rb = gameObject.AddComponent<Rigidbody>();
		else if (rb == null)
			rb = gameObject.GetComponent<Rigidbody>();
		//disabeling gravity because we are handeling it by ourselfs
		rb.useGravity = false;
		
		//Camera
		if (cameraHolder == null)
			cameraHolder = new GameObject("cameraHolder").transform;
		//check if under cameraholder is camera if not to create
		cam = cameraHolder.Find("Camera");
		if (cam == null)
		{
			cam = new GameObject("Camera").transform;
			cam.gameObject.AddComponent<Camera>();
			cam.parent = cameraHolder;
		}
		
		//orientation
		if (orientation == null)
		{
			orientation = new GameObject("orientation").transform;
			orientation.position = objectWithModel.transform.GetComponent<Renderer>().bounds.center;
			orientation.parent = transform;
		}
		
		//set automaticaly distance for ground check by picking half of the mesh size and adding 0.1f (works perfectly if orientation os in the middle of mesh)
		if (groundCheckLength == 0)
			groundCheckLength = (rend.bounds.size.y/2) + 0.01f;
		//set automaticaly size for ground check by picking half of the mesh size
		if (groundCheckSize == Vector3.zero)
			groundCheckSize = new Vector3((rend.bounds.size.x/2) - 0.2f, 0.01f, (rend.bounds.size.y/2) - 0.2f);
		
		//jump delay can't be zero
		if (jumpDelay <= 0)
			jumpDelay = 0.1f;
		
		//debugText
		if (debugText == true)
		{
			GameObject DTP = Instantiate(debugTextPrefab);
			realSpeedText = DTP.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			desiredSpeedText = DTP.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
			rigidbodySpeedText = DTP.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
			stateText = DTP.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
		}
	}
	
	private void Update()
	{
		otherUpdate();
		CameraUpdate();
		GroundCheck();
		GravitationUpdate();
		MyInput();
		StateHandler();
		SpeedLimitation();
		SmoothSpeed();
		DebugText();
	}
	
	private void FixedUpdate()
	{
		Move();
		
		//apply gravity. If jumping for first 0.1 s use normalGravitation
		if (jumpedTimer > 0)
			rb.AddForce(-currUp * -normalGravitation, ForceMode.Acceleration);
		else
			rb.AddForce(-currUp * -gravitation, ForceMode.Acceleration);
	}
	
	private void otherUpdate()
	{
		if (realGrounded)
			rb.drag = groundDrag;
		else
			rb.drag = 0;
		
		if (jumpRememberTimer > 0)
			jumpRememberTimer -= Time.deltaTime;
		if (jumpDelayTimer > 0)
			jumpDelayTimer -= Time.deltaTime;
		//to track first frames after jump
		if (jumpedTimer > 0)
		{
			//also set drag to zero if ground check do not be on time
			rb.drag = 0;
			jumpedTimer -= Time.deltaTime;;
		}
	}
	
	private void StateHandler()
	{
		if (!noInput && realGrounded) //walk
		{
			moveType = movementTypes.walking;
			desiredSpeed = walkSpeed;
		}
		else if (noInput && realGrounded) //stand
		{
			moveType = movementTypes.stand;
			desiredSpeed = 0;
		}
		else //air
		{
			if (noInput)
				desiredSpeed = 0;
			else if (desiredSpeed == 0)
				desiredSpeed = walkSpeed;
			moveType = movementTypes.air;
		}
	}
	
	private void MyInput()
	{
		noInput = (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0);
		
		//jump
		if ((holdJumpButton && Input.GetKey(jumpKey)) || (!holdJumpButton && Input.GetKeyDown(jumpKey)))
			Jump(false);
		
		if (jumpRememberTimer > 0)
			Jump(true);
		
		if (jumpCut && Input.GetKeyUp(jumpKey) && rb.velocity.y > 0)
			rb.AddForce(Vector3.down * rb.velocity.y * jumpCutStrength, ForceMode.Impulse);
	}
	
	private void Move()
	{
		//get move direction
		if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
			moveDirection = orientation.forward * Input.GetAxisRaw("Vertical") + orientation.right * Input.GetAxisRaw("Horizontal");
		
		/*apply move direction (multiplicator 10 can be changed to any big number.
		It is here because we handle speed by limiting it in SpeedLimitation())*/
		rb.AddForce(realSpeed * 10 * moveDirection.normalized, ForceMode.Force);
	}
	
	#region Jump
	//return true if jump was sucesfull
	bool Jump(bool byRemember)
	{
		//basic jump
		if (grounded && jumpDelayTimer <= 0)
		{
			jumpAmm = ammountOfJumpsInAir;
			
			//set delay to next jumo
			jumpDelayTimer = jumpDelay;
			//for setting drag to 0 (because groundcheck do not affected by jump)
			jumpedTimer = 0.1f;
			//jumpedTimer works only after that frame so we need to set drag to 0 in that frame too
			rb.drag = 0;
			
			AddJumpVelocity(jumpHeight);
			
			//speed manipulations
			//if desiredSpeed 9.7, maxObtainableSpeedByJump is 10 and increaseSpeedAfterJumpBy is 0.5 it would add 0.3
			desiredSpeed += Mathf.Clamp(maxObtainableSpeedByJump - desiredSpeed, 0, increaseSpeedAfterJumpBy);
			
			return true;
		}
		//dubble jump (can be triggered only by keyDown and can't be trigered by remember[Input.GetKeyDown(jumpKey) imply that])
		else if (jumpAmm > 0 && jumpDelayTimer <= 0 && Input.GetKeyDown(jumpKey))
		{
			jumpAmm--;
			
			//set delay to next jumo
			jumpDelayTimer = dubbleJumpDelay;
			//for setting drag to 0 (because groundcheck do not affected by jump)
			jumpedTimer = 0.1f;
			//jumpedTimer works only after that frame so we need to set drag to 0 in that frame too
			rb.drag = 0;
			
			AddJumpVelocity(dubbleJumpHeight);
			
			return true;
		}
		
		//if called by remember do not add remember time
		else if (!byRemember)
			jumpRememberTimer = jumpRememberTime;
		
		return false;
	}
	
	void AddJumpVelocity(float addJumpHeight)
	{
		//we are using mormalGravitation instead of gravitation because after jump velocity is always positive
		float desiredJumpHeight = addJumpHeight;
		//calculating height we need to add from that point to desired point by calculating jump height from velocity that we currently have
		if (rb.velocity.y > 0) //if we are falling we do not want to decrees our jumpImpulse
			desiredJumpHeight += ((rb.velocity.y * rb.velocity.y) / -normalGravitation) / 2; //jumpHeight = (Vtakeoff² / -gravitation) / 2
		//reseting velocity by y because we included current velocity in desiredJumpHeight
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		//calculating takeoff velocity independent of mass to get to desired height
		rb.AddForce(Vector3.up * Mathf.Sqrt((-2 * normalGravitation) * desiredJumpHeight), ForceMode.VelocityChange);
	}
	#endregion
	
	#region gravitation
	void GravitationUpdate()
	{
		if (rb.velocity.y < 0)
			gravitation = fallingGravitation;
		else
			gravitation = normalGravitation;
	}
	#endregion
	
	#region Speed
	private void SpeedLimitation()
	{
		//calculate speed independend of fall speed
		Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		//sqrmagnitude to save sqrt call
		if (flatVelocity.sqrMagnitude > realSpeed*realSpeed)
		{
			//calculate max speed with current move direction
			Vector3 limitedVelocity = flatVelocity.normalized * realSpeed;
			//apply limited velocity to player's speed without changing it's y speed
			rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
		}
	}
	
	private bool drasticSpeedRunning = false;
	private void SmoothSpeed()
	{
		/* speed rules */
		if (noInput && noInputNoSpeedGround && realGrounded)
		{
			realSpeed = 0;
			return;
		}
		else if (noInput && noInputNoSpeedAir && !realGrounded)
		{
			realSpeed = 0;
			return;
		}
		
		if (!smoothSpeed)
		{
			if (lowerSpeedInAir && upperSpeedInAir)
				realSpeed = desiredSpeed;
			else if (lowerSpeedInAir && realSpeed > desiredSpeed)
				realSpeed = desiredSpeed;
			else if (upperSpeedInAir && realSpeed < desiredSpeed)
				realSpeed = desiredSpeed;
			
			return;
		}
		
		
		//if difference between desired and real, drastic enough                                            lowering    upering      if not started smooth getting to desired speed start it
		if (drasticSmoothSpeed && Mathf.Abs(realSpeed - desiredSpeed) > ((realSpeed - desiredSpeed) >= 0 ? drastic.x : drastic.y) && !drasticSpeedRunning)
		{
			StartCoroutine(drasticSpeed());
			return;
		}
		//if difference not big enough to start smooth getting to desired speed and it not active set speed to desired
		else if (drasticSmoothSpeed && !drasticSpeedRunning)
		{
			realSpeed = desiredSpeed;
			return;
		}
		//always not go further in function if drastic
		else if (drasticSmoothSpeed)
			return;
		
		//if smoothSpeed and !drasticSmoothSpeed
		if (lastDesiredSpeed != desiredSpeed)
		{
			lastLastDesiredSpeed = realSpeed;
			//calculating speed for deltaSpeedCounter by proportion (desiredSpeed - lastDesiredSpeed)/1 = (desiredSpeed - realSpeed)/x
			deltaSpeedCounterSpeed = 2 - ((desiredSpeed - realSpeed) / (desiredSpeed - lastDesiredSpeed));
			deltaSpeedCounter = 0;
		}
		lastDesiredSpeed = desiredSpeed;
		
		//if smoothspeed and !drasticSmoothSpeed
		//choose what variable we should use for speed changing
		float smoothWay = realGrounded ? (realSpeed < desiredSpeed ? smoothSpeedAccelerationGround.x : smoothSpeedAccelerationGround.y) : (realSpeed < desiredSpeed ? smoothSpeedAccelerationAir.x : smoothSpeedAccelerationAir.y);
		
		//increase deltaSpeedCounter by smothWay and multiplay by deltaSpeedCounterSpeed because we want to count what player speed was when he changes desiredSpeed
		deltaSpeedCounter += Mathf.Clamp(1 - deltaSpeedCounter, 0, (Time.deltaTime * smoothWay) * deltaSpeedCounterSpeed);
		
		if (!realGrounded) //if in air
		{
			//if lower speed
			if (lowerSpeedInAir && realSpeed > desiredSpeed)
			{
				realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
			}
			//if upper speed
			else if (upperSpeedInAir && realSpeed < desiredSpeed)
			{
				realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
			}
		}
		else //if grounded
			realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
	}
	private IEnumerator drasticSpeed()
	{
		//for keeping track of how many times speed changed 
		bool once = true;
		//set this variable to know if this coroutine is still running
		drasticSpeedRunning = true;
		//while this variables is almost done
		while (Mathf.Round(realSpeed) != Mathf.Round(desiredSpeed))
		{
			if (lastDesiredSpeed != desiredSpeed)
			{
				lastLastDesiredSpeed = realSpeed;
				//calculating speed for deltaSpeedCounter by proportion (desiredSpeed - lastDesiredSpeed)/1 = (desiredSpeed - realSpeed)/x
				deltaSpeedCounterSpeed = 2 - ((desiredSpeed - realSpeed) / (desiredSpeed - lastDesiredSpeed));
				deltaSpeedCounter = 0;
				
				if (!once)
					break;
				once = false;
			}
			lastDesiredSpeed = desiredSpeed;
			
			//choose what variable we should use for speed changing
			float smoothWay = realGrounded ? (realSpeed < desiredSpeed ? smoothSpeedAccelerationGround.x : smoothSpeedAccelerationGround.y) : (realSpeed < desiredSpeed ? smoothSpeedAccelerationAir.x : smoothSpeedAccelerationAir.y);
											
			//increase deltaSpeedCounter by smothWay and multiplay by deltaSpeedCounterSpeed because we want to count what player speed was when he changes desiredSpeed
			deltaSpeedCounter += Mathf.Clamp(1 - deltaSpeedCounter, 0, (Time.deltaTime * smoothWay) * deltaSpeedCounterSpeed);
			
			if (!realGrounded) //if in air
			{
				//if lower speed
				if (lowerSpeedInAir && realSpeed > desiredSpeed)
				{
					realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
				}
				//if upper speed
				else if (upperSpeedInAir && realSpeed < desiredSpeed)
				{
					realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
				}
			}
			else //if grounded
				realSpeed = Mathf.Lerp(lastLastDesiredSpeed, desiredSpeed, deltaSpeedCounter);
			
			yield return null;
		}
		drasticSpeedRunning = false;
	}
	#endregion
	
	#region GroundCheck
	private void GroundCheck()
	{
		//reset variables
		realGrounded = false;
		grounded = false;
		
		//set grounded if cayoty time
		if (useCayotyTime)
		{
			if (cayotyTime > 0)
				cayotyTime -= Time.deltaTime;
			grounded = cayotyTimer > 0;
		}
		
		//cast box to see if under player is objects on what he can stay
		RaycastHit groundHit;
		if (Physics.BoxCast(orientation.position, groundCheckSize, -orientation.up, out groundHit, Quaternion.identity, groundCheckLength, groundLayer))
		{
			if (useCayotyTime)
				cayotyTimer = cayotyTime;
			else
				grounded = true;
			realGrounded = true;
		}
	}
	#endregion
	
	#region Camera
	private void CameraUpdate()
	{
		//set cameraholder's position to cameraPosition (child of player)
		cameraHolder.position = cameraPosition.position;
		//inversed because this is how quaternions work
		cameraRotationX -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensativity.y;
		cameraRotationY += Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensativity.x;
		//min and max look angle
		cameraRotationX = Mathf.Clamp(cameraRotationX, maxLookAngle.x, maxLookAngle.y);
		//apply rotation
		orientation.rotation = Quaternion.Euler(0, cameraRotationY, 0);
		cameraHolder.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, cameraRotationZ);
	}
	
	public void MouseDisable()
	{
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	public void MouseEnable()
	{
		Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
	}
	#endregion
	
	#region Utilities
	//Utilities
	private float WrapAngle(float angle)
    {
        angle %= 360;
        if(angle > 180)
            return angle - 360;
        return angle;
    }
	float myRound(float num, int x)
	{
		return Mathf.Round(num * (10*x)) / (10*x);
	}
	#endregion
	
	#region DebugInformation
	void DebugText()
	{
		if (!debugText)
			return;
		
		realSpeedText.text = myRound(realSpeed, 2).ToString();
		desiredSpeedText.text = desiredSpeed.ToString();
		rigidbodySpeedText.text = myRound(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude, 2).ToString();
		stateText.text = movementTypes.GetName(typeof(movementTypes), moveType);
	}
	#endregion

	#region Gizmos
	void OnDrawGizmosSelected()
	{
		if (!gizmosSelected)
			return;
		
		if (groundCheck)
			DrawGroundCheck();
	}
	
	void OnDrawGizmos()
	{
		if (!gizmosAllways)
			return;
		
		if (groundCheck)
			DrawGroundCheck();
	}
	
	private void DrawGroundCheck()
	{
		//if orientation isn't set up
		if (orientation == null)
			return;
		
		//Draw a Ray down from orientation toward the maxLength
        Gizmos.DrawRay(orientation.position, -orientation.up * groundCheckLength);
        //Draw a cube where the hit exists
        Gizmos.DrawWireCube(orientation.position + -orientation.up * groundCheckLength, groundCheckSize);
	}
	#endregion
}
