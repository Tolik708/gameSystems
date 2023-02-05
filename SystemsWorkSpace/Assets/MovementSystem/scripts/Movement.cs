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
	[SerializeField] private float   walkSpeed =           5;
	[SerializeField] private bool    lowerSpeedInAir =     false;
	[SerializeField] private bool    upperSpeedInAir =     true;
	[SerializeField] private bool    noInputNoSpeedInAir = true;
	[SerializeField] private bool    smoothSpeed =         false;
	[SerializeField] private Vector2 smoothSpeedSpeed =    new Vector2(10, 10); // lowering / upering
	[SerializeField] private bool    drasticSmoothSpeed =  false;
	[SerializeField] private Vector2 drastic =             new Vector2(10, 10); // lowering / upering
	
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
	private float realSpeed;
	private float desiredSpeed;
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
	
	private void Start()
	{
		/*all the variables here are seting automaticaly*/
		
		//disable mouse when in player
		MouseDisable();

		//get renderer for later use
		Renderer rend = objectWithModel.transform.GetComponent<Renderer>();
		
		//Rigidbody
		if (rb == null && gameObject.GetComponent<Rigidbody>() == null)
			rb = gameObject.AddComponent<Rigidbody>();
		else if (rb == null)
			rb = gameObject.GetComponent<Rigidbody>();
		
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
		MyInput();
		StateHandler();
		SpeedLimitation();
		SmoothSpeed();
		DebugText();
	}
	
	private void FixedUpdate()
	{
		Move();
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
		if (!noInput && realGrounded)
		{
			moveType = movementTypes.walking;
			desiredSpeed = walkSpeed;
		}
		else if (noInput && realGrounded)
		{
			moveType = movementTypes.stand;
			desiredSpeed = walkSpeed;
		}
		else
		{
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
		//get move direction (if !movePlayerIfRealSpeedNotNull)
		Vector3 moveDirection = orientation.forward * Input.GetAxisRaw("Vertical") + orientation.right * Input.GetAxisRaw("Horizontal");
		
		/*apply move direction (multiplicator 100 can be changed to any big number.
		It is here because we handle speed by limiting it in SpeedLimitation())*/
		rb.AddForce(moveDirection.normalized * realSpeed * 10, ForceMode.Force);
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
			
			//reset y velocity if it lower 0 for jump be independend of y velocity (because can be grounded earlier than velocity.y is 0)
			if (rb.velocity.y < 0)
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			//adding 0.2 for sure that we get to needed height (can be changed)
			//calculating impulse independent of mass to get to needed height
			rb.AddForce(Vector3.up * (Mathf.Sqrt((-2 * -9.81f) * jumpHeight) + 0.2f), ForceMode.VelocityChange);
			
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
			
			//reset y velocity if it lower 0 for jump be independend of y velocity (because can be grounded earlier than velocity.y is 0)
			if (rb.velocity.y < 0)
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			//calculating impulse independent of mass to get to needed height
			rb.AddForce(Vector3.up * Mathf.Sqrt((-2 * -9.81f) * dubbleJumpHeight), ForceMode.VelocityChange);
		}
		
		//if caused by remember do not add remember time
		else if (!byRemember)
			jumpRememberTimer = jumpRememberTime;
		
		return false;
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
		if (noInput && noInputNoSpeedInAir)
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
		
		//if smoothSpeed
		//if difference between desired and real drastic enough                                            lowering    upering
		if (drasticSmoothSpeed && Mathf.Abs(realSpeed - desiredSpeed) > ((realSpeed - desiredSpeed) >= 0 ? drastic.x : drastic.y))
		{
			//if not started smooth getting to desired speed start it
			if (drasticSpeedRunning == false)
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
		
		if (!realGrounded)
		{
			if (lowerSpeedInAir && upperSpeedInAir)
			{
				//if lower speed
				if (realSpeed > desiredSpeed)
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
				//if upper speed
				if (realSpeed < desiredSpeed)
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
			}
			else if (lowerSpeedInAir && realSpeed > desiredSpeed)
			{
				//if lower speed
				realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
			}
			else if (upperSpeedInAir && realSpeed < desiredSpeed)
			{
				//if upper speed
				realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
			}
		}
		else
		{
			//if lower speed
			if (realSpeed > desiredSpeed)
				realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
			//if upper speed
			if (realSpeed < desiredSpeed)
				realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
		}
	}
	private IEnumerator drasticSpeed()
	{
		//set this variable to know if this coroutine is still running
		drasticSpeedRunning = true;
		//while this variables is almost done
		while (Mathf.Round(realSpeed) != Mathf.Round(desiredSpeed))
		{
			//if in air check if we need to change speed
			if (!realGrounded)
			{
				if (lowerSpeedInAir && upperSpeedInAir)
				{
					//if lower speed
					if (realSpeed > desiredSpeed)
						realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
					//if upper speed
					else if (realSpeed < desiredSpeed)
						realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
				}
				else if (lowerSpeedInAir && realSpeed > desiredSpeed)
				{
					//if lower speed
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
				}
				else if (upperSpeedInAir && realSpeed < desiredSpeed)
				{
					//if upper speed
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
				}
			}
			//always change speed on ground
			else
			{
				//if lower speed
				if (realSpeed > desiredSpeed)
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.x);
				//if upper speed
				else if (realSpeed < desiredSpeed)
					realSpeed = Mathf.Lerp(realSpeed, desiredSpeed, Time.deltaTime * smoothSpeedSpeed.y);
			}
			
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
