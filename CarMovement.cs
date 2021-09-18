using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

// Комментарии скоро будут переведены на английский
// Основной скрипт, отвечающий за разгон, торможение, повороты автомобиля
public class CarMovement : MonoBehaviour
{
    // Оси автомобиля
    public Axle[] carAxle = new Axle[2];
    // Коллайдеры колес, реализующие
    // физическое взаймодействие колёс
    // с внешним миром
    public WheelCollider[] wheelColliders;

    // значение крутящего момента,
    // устанавливаемое пользователем
    public float initial_te;

    // крутящий момент
    float turningEffect;

    // угол поворота
    public float steerAngle;

    // центр масс. Чем ниже центр масс,
    // тем более авто устойчиво на дороге,
    // меньше кренится и имеет меньшую вероятность
    // перевернуться
    public Transform centerOfMass;
    [Range(0, 1)]
    public float steerHelpValue = 0;

    // Умножение на данный угол
    // позволяет изменить ось,
    // вокруг которой вращается колесо.
    public Vector3 additionalWheelAngle;

    // переменная, позволяющая
    // реализовать поворот авто
    private float horInput;

    // переменная, позволяющая
    // реализовать педаль "газа"
    static int verInput;

    // Передача. АКПП
    public static int gear = 0;

    // Компонент авто, который
    // отвечает за физическое
    // взаимодействие с внешним миром
    Rigidbody rb;

    bool onGround;
    float lastYRotation;
    // Руль
    public Transform helm;
    Quaternion startHelmRotation;

    // сила инерции. Определяет,
    // как быстро авто теряет скорость,
    // когда педаль "газа" отпущена
    int inertiaForce = 1560;

    // сила обычных тормозов
    float usualBrakeForce = 1000.0f;

    bool beginning = true;

    // кнопки коробки передач
    public SC_ClickTracker[] clickButtons = new SC_ClickTracker[3];
    // кнопки коробки передач
    public GameObject[] buttons = new GameObject[3];
    //public Transform[] callipers = new Transform[2];

    float time1 = 0;
    float time2 = 0;
    bool brakeByEngine = false;

    public Material stopLight;
    
    public Material rearLight;

    public Light[] rearLightSpots;
    public static Driving_VAZ contoller;


    // Метод, необходимый для первоначальной инициализации.
    // Вызывается один раз при запуске игры.
    private void Start()
    {
        // Получение компонента авто, который
        // отвечает за физическое взаимодействие
        // с внешним миром
        rb = GetComponent<Rigidbody>();

        // Задание центра масс авто
        rb.centerOfMass = centerOfMass.localPosition;

        // Задание крутящего момента
        turningEffect = initial_te;

        // Задание вращения руля
        startHelmRotation = helm.localRotation;

       
        foreach (Light light in rearLightSpots)
        {
            light.intensity = 0f;
        }

        //TurnOffRearLights();
        contoller = this;
    }

    // Метод вызывающийся фиксированное количество раз
    // за секунду. В данном случае 40 раз (40 fps).
    void FixedUpdate()
    {
        horInput = Input.GetAxis("Horizontal");

        // Случай включения нейтральной передачи
        if (SC_MobileControls.instance.GetMobileButton("Neutral Gear"))
        {
            gear = 1;
            clickButtons[0].ChangeHoldingStatus();
            buttons[0].SetActive(false);
            buttons[1].SetActive(true);
        }

        if (SC_MobileControls.instance.GetMobileButton("Drive Gear"))
        {
            gear = -1;
            clickButtons[1].ChangeHoldingStatus();
            buttons[1].SetActive(false);
            buttons[2].SetActive(true);
            EnableRearLight();
            foreach (Light light in rearLightSpots)
            {
                light.intensity = 2f;
            }
        }

        if (SC_MobileControls.instance.GetMobileButton("Rear Gear"))
        {
            gear = 0;
            clickButtons[2].ChangeHoldingStatus();
            buttons[2].SetActive(false);
            buttons[0].SetActive(true);
            DisableRearLight();
            foreach (Light light in rearLightSpots)
            {
                light.intensity = 0f;
            }
        }

        // Нажатие/ ненажатие педали газа
        if (SC_MobileControls.instance.GetMobileButton("GasPedal") || (Input.GetKey(KeyCode.UpArrow)))
        {
            verInput = 1;
            beginning = false;
        }
        else
        {
            verInput = 0;
        }

        CheckOnGround();

        // Вызов метода, реализующего
        // движение авто
        Accelerate();
        
        SteerHelpAssist();
    }

    // Метод, реализующий движение авто
    void Accelerate()
    {
        // Устанавливаем начальные значения
        // для предотвращения блокировки колёс.
        if (((carAxle[0].rightWheel.brakeTorque != 0) && (verInput == 0)) ||
               ((carAxle[1].rightWheel.brakeTorque != 0) && (verInput == 0)))
        {
            //Debug.Log("Соси ХУЕЦ");
            SetDefaultValues();
        }
        // Если вращаются колёса(авто едет),
        // и педаль газа не нажата, то
        // к колёсам применятся сила инерции
        else if ((carAxle[0].rightWheel.rpm > 1) &&
             (carAxle[1].rightWheel.rpm > 1) &&
               (verInput == 0))
        {
            Debug.Log("Инерция");
            ApplyInertiaForce();
        }
        else if ((carAxle[0].rightWheel.rpm < 0) &&
             (carAxle[1].rightWheel.rpm < 0) &&
               (verInput == 0))
        {
            ApplyInertiaForce();
        }
     
        // цикл, проходящий по всем осям и колёсам
        foreach (Axle axle in carAxle)
        {
            // если колёса данной оси способны поворачивать
            if (axle.steering)
            {
                //Получение положения мобильного устройства в пространстве
                //Vector3 tilt = Quaternion.Euler(90, 0, 0) * Input.acceleration;

                // если устройство наклонено влево,
                // то авто поворачивает влево
                if ((tilt.x < 0.1) && (tilt.z < -0.2))
                {
                    horInput = tilt.x;
                }

                //if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.LeftArrow))
                //{
                //    //Debug.Log("[eq");
                //    horInput = Input.GetAxis("Horizontal");
                //}

                // если устройство наклонено вправо,
                // то авто поворачивает вправо
                if ((tilt.x > 0.1) && (tilt.z < -0.2))
                {
                    horInput = tilt.x;
                }

                // если устройство не наклонено,
                // то авто едет прямо
                if ((tilt.x >= 0.0) && (tilt.x < 0.1))
                {
                    horInput = 0;
                }

                // Применить к колёсам вычисленные повороты
                axle.rightWheel.steerAngle = steerAngle * horInput;
                axle.leftWheel.steerAngle = steerAngle * horInput;
                Debug.Log("Поворот: "+axle.rightWheel.steerAngle);
            }
            // если колёсам данной оси
            // был передан крутящий момент
            if (axle.motor)
            {

                //float lastRpm = 0;
                if ((verInput == 0) && (beginning == true))
                {
                    time1 = Time.realtimeSinceStartup;
                }

                // если нажата педаль "газа"
                if ((verInput == 1 && gear == 1) || (verInput == 1 && gear == -1))
                {
                    axle.rightWheel.motorTorque = gear * turningEffect;
                    axle.leftWheel.motorTorque = gear * turningEffect;

                }
                else 
                {
                    axle.rightWheel.motorTorque = 0f;
                    axle.leftWheel.motorTorque = 0f;
                }               
            }

            if ((axle.rightWheel.rpm > 837) && (axle.rightWheel.rpm < 840))          
            {

                time2 = Time.realtimeSinceStartup;
                float time3 = time2 - time1;

            }
            Debug.Log("rpm " + carAxle[0].rightWheel.rpm);
            if (carAxle[0].rightWheel.rpm > 1257.0f)
            {
                //Debug.Log("МАКС");
            }
            Debug.Log("verinput "+verInput);
            
            // Вызов метода, который вращает колёса(их физические модели)
            rotateMeshWheels(axle.rightWheel, axle.meshRightWheel);
            rotateMeshWheels(axle.leftWheel, axle.meshLeftWheel);
        }

        // Если нажата педаль тормоза, то применить тормоза
        if (SC_MobileControls.instance.GetMobileButton("Brakes") || (Input.GetKey(KeyCode.Space)))
        {
            stopLight.EnableKeyword("_EMISSION");
       
            // Вызов метода, который применяет тормоза
            ApplyUsualBrakes();
        }
        else
        {
            
            stopLight.DisableKeyword("_EMISSION");
        }

        // Поворот руля в соответствии с поворотами колёс
        helm.localRotation = startHelmRotation * Quaternion.Euler(Vector3.forward * 180 * -horInput / 2);
    }

    // Реализация движения физической модели колеса
    void rotateMeshWheels(WheelCollider col, Transform meshWheel)
    {
        // Положение коллайдера колеса в пространстве
        Vector3 position;
        // Поворот коллайдера колеса.
        Quaternion rotation;

        // Получение координат коллайдера колеса. 
        col.GetWorldPose(out position, out rotation);

        // Присвоить положению физической модели колеса
        // вычисленное положение коллайдера колеса в пространстве.
        meshWheel.position = position;

        // Исправление вращения физической модели колеса, которая изначально
        // вращается вокруг неправильной оси. Если не исправлять, то 
        // физическая модель колеса будет вращаться не так,
        // как в реальной жизни, а в другой плоскости.
        // Несмотря на то, что неправильное, с точки зрения наблюдателя,
        // вращение физической модели колеса не влияет на движение авто,
        // поскольку только коллайдеры отвечают за физическое взаимодействие
        // с поверхностью, по которой движется авто, исправлять вращение
        // физическое модели колеса необходимо для благообразия и
        // соотвествия действительности.
        meshWheel.rotation = rotation * Quaternion.Euler(additionalWheelAngle);
    }

    void SteerHelpAssist()
    {
        if (!onGround)
            return;

        if (Mathf.Abs(transform.rotation.eulerAngles.y - lastYRotation) < 50f)
        {
            float turnAdjust = (transform.rotation.eulerAngles.y - lastYRotation) * steerHelpValue;
            Quaternion rotateHelp = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            rb.velocity = rotateHelp * rb.velocity;
        }
        lastYRotation = transform.rotation.eulerAngles.y;
    }

    void CheckOnGround()
    {
        onGround = true;
        foreach (WheelCollider wheelCol in wheelColliders)
        {
            if (!wheelCol.isGrounded)
                onGround = false;
        }
    }

    // Метод, активирующий тормоза
    public void ApplyUsualBrakes()
    {
        // Если включен режим "Drive"(АКПП) или нейтральная передача
        // и количество оборотов коллайдера колеса в минуту 
        // больше 1, то есть авто движется вперед, то присвоить силе 
        // тормозов каждого коллайдера колеса данное значение
        if (((gear == 1) || (gear == 0)) &&
              (carAxle[0].rightWheel.rpm > 1) &&
                (carAxle[0].leftWheel.rpm > 1) &&
                  (carAxle[1].rightWheel.rpm > 1) &&
                    (carAxle[1].leftWheel.rpm > 1))
        {
            // Присвоение силе тормозов коллайдера правого колеса передней оси данного значения
            carAxle[0].rightWheel.brakeTorque = 3.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера левого колеса передней оси данного значения
            carAxle[0].leftWheel.brakeTorque = 3.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера правого колеса задней оси данного значения
            carAxle[1].rightWheel.brakeTorque = 3.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера левого колеса задней оси данного значения
            carAxle[1].leftWheel.brakeTorque = 3.0f * usualBrakeForce;
        }
        // Если включен задняя передача
        // и количество оборотов коллайдера колеса в минуту 
        // больше 0, то есть авто движется назад, то присвоить силе 
        // тормозов каждого коллайдера колеса данное значение
        else if ((gear == -1) &&
                  (carAxle[0].rightWheel.rpm < 0) &&
                    (carAxle[0].leftWheel.rpm < 0) &&
                      (carAxle[1].rightWheel.rpm < 0) &&
                        (carAxle[1].leftWheel.rpm < 0))
        {
            // Присвоение силе тормозов коллайдера правого колеса передней оси данного значения
            carAxle[0].rightWheel.brakeTorque = 2.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера левого колеса передней оси данного значения
            carAxle[0].leftWheel.brakeTorque = 2.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера правого колеса задней оси данного значения
            carAxle[1].rightWheel.brakeTorque = 2.0f * usualBrakeForce;
            // Присвоение силе тормозов коллайдера левого колеса задней оси данного значения
            carAxle[1].leftWheel.brakeTorque = 2.0f * usualBrakeForce;
        }
    }

    // Метод, активирующий инерцию
    public void ApplyInertiaForce()
    {
        // Инерция реализована через тормоза коллайдера колеса.
        // Отличие в том, что в случае инерции используется
        // меньшая сила тормозов по сравнению с реальным тормо-
        // жением посредством нажатия педали тормоза

        // Обнуляем крутящий момент, передаваемый коллайдерам колес.
        // Если этого не сделать, количество оборотов 
        // коллайдеров в минуту будет расти, и 
        // авто будет разгоняться. В то же время
        // педаль "газа" не нажата.

        // Обнулить крутящий момент, передающийся на правое колесо передней оси
        carAxle[0].rightWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на левое колесо передней оси
        carAxle[0].leftWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на правое колесо задней оси
        carAxle[1].rightWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на левое колесо задней оси
        carAxle[1].leftWheel.motorTorque = 0;


        // Присвоение силе тормозов коллайдера правого колеса
        // передней оси данного значения
        carAxle[0].rightWheel.brakeTorque = inertiaForce;
        // Присвоение силе тормозов коллайдера левого колеса
        // передней оси данного значения
        carAxle[0].leftWheel.brakeTorque = inertiaForce;
        // Присвоение силе тормозов коллайдера правого колеса
        // задней оси данного значения
        carAxle[1].rightWheel.brakeTorque = inertiaForce;
        // Присвоение силе тормозов коллайдера левого колеса
        // задней оси данного значения
        carAxle[1].leftWheel.brakeTorque = inertiaForce;

      

        Debug.Log("Н-ЛЬ");
    }

    // Установка начальных значений
    public void SetDefaultValues()
    {
        // Обнулить силу тормозов коллайдера правого колеса задней оси
        carAxle[0].rightWheel.brakeTorque = 0;
        // Обнулить силу тормозов коллайдера левого колеса задней оси
        carAxle[0].leftWheel.brakeTorque = 0;
        // Обнулить силу тормозов коллайдера правого колеса задней оси
        carAxle[1].rightWheel.brakeTorque = 0;
        // Обнулить силу тормозов коллайдера левого колеса задней оси
        carAxle[1].leftWheel.brakeTorque = 0;

        // Обнулить крутящий момент, передающийся на правое колесо передней оси
        carAxle[0].rightWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на левое колесо передней оси
        carAxle[0].leftWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на правое колесо задней оси
        carAxle[1].rightWheel.motorTorque = 0;
        // Обнулить крутящий момент, передающийся на левое колесо задней оси
        carAxle[1].leftWheel.motorTorque = 0;
    }

    void EnableRearLight()
    {
        rearLight.EnableKeyword("_EMISSION");
        rearLight.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));
    }

    void DisableRearLight()
    {
        rearLight.DisableKeyword("_EMISSION");
        rearLight.color = new Color(1f, 1f, 1f, 1f);
    }

}

[System.Serializable]
// 
public class Axle
{
    // Коллайдер правого колеса
    public WheelCollider rightWheel;
    // Коллайдер левого колеса
    public WheelCollider leftWheel;

    // Физическая модель правого колеса
    public Transform meshRightWheel;
    // Физическая модель левого колеса
    public Transform meshLeftWheel;

    // Переменная, определяющая, могут ли
    // коллайдеры колёс данной оси поворачивать
    public bool steering;

    // Переменная, определяющая, будет
    // ли передаваться крутящий момент
    // на коллайдеры колёс данной оси
    public bool motor;
}
