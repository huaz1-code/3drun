using UnityEngine;

/// <summary>
/// FinalPlatformManager脚本 - 控制平台生成的终点逻辑
/// 功能说明：
/// 1. 监听平台生成数量
/// 2. 当生成到第20个平台时停止平台生成
/// 3. 在第20个平台位置生成cube（终点标记）
/// 4. 支持两种方式：使用预制体Instantiate创建，或使用场景中的对象（初始隐藏，到达时显示）
/// </summary>
public class finalplatformManager : MonoBehaviour
{
    /// <summary>
    /// 目标平台数量 - 到达此数量时停止生成
    /// 当平台生成到这个数量时触发终点逻辑
    /// </summary>
    [Tooltip("目标平台数量")]
    public int targetPlatformCount = 20;

    /// <summary>
    /// Cube对象 - 可以是场景中的对象或预制体
    /// 如果是场景中的对象：开局自动隐藏，到达时显示并移动到平台位置
    /// 如果是预制体：到达时Instantiate创建
    /// </summary>
    [Tooltip("Cube对象（可以是场景中的对象或预制体）")]
    public GameObject cubeObject;

    /// <summary>
    /// 位置偏移 - cube相对于第20个平台中心的偏移量
    /// x：左右偏移（正值向右，负值向左）
    /// y：上下偏移（正值向上，负值向下）
    /// z：前后偏移（正向前方，负值向后方）
    /// </summary>
    [Tooltip("Cube相对于平台中心的位置偏移（x左右/y上下/z前后）")]
    public Vector3 positionOffset = new Vector3(0f, 2f, 0f);

    /// <summary>
    /// 平台生成器引用 - 用于控制平台生成
    /// 需要获取PlatformSpawner组件来停止生成
    /// </summary>
    private PlatformSpawner platformSpawner;

    /// <summary>
    /// 终点是否已生成标记
    /// 防止重复生成cube
    /// </summary>
    private bool endPointGenerated = false;

    /// <summary>
    /// cube是否是场景中的对象（非预制体）
    /// </summary>
    private bool isSceneObject = false;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：查找PlatformSpawner组件，初始化状态，隐藏场景中的cube
    /// </summary>
    void Start()
    {
        // 查找PlatformSpawner组件
        platformSpawner = FindObjectOfType<PlatformSpawner>();
        if (platformSpawner == null)
        {
            Debug.LogError("finalplatformManager: 未找到PlatformSpawner组件！");
            enabled = false;
            return;
        }

        // 检查是否设置了cubeObject
        if (cubeObject == null)
        {
            Debug.LogError("finalplatformManager: 请在Inspector中设置cubeObject！");
        }
        else
        {
            // 判断cubeObject是场景中的对象还是预制体
            // 如果对象的场景不为null且不是Prefab，则是场景中的对象
            if (cubeObject.scene.name != null && !cubeObject.scene.name.Equals(""))
            {
                isSceneObject = true;
                // 隐藏场景中的cube对象，到达时再显示
                cubeObject.SetActive(false);
                Debug.Log("finalplatformManager: 场景中的cube已隐藏，等待到达第20个平台");
            }
            else
            {
                isSceneObject = false;
                Debug.Log("finalplatformManager: 使用预制体模式，将在到达时Instantiate");
            }
        }

        endPointGenerated = false;
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// 职责：检查平台数量，到达目标时执行终点逻辑
    /// </summary>
    void Update()
    {
        // 如果终点已生成，不再执行
        if (endPointGenerated) return;

        // 获取当前平台索引（PlatformSpawner中的platformIndex）
        // 使用反射获取私有字段
        System.Reflection.FieldInfo fieldInfo = typeof(PlatformSpawner).GetField("platformIndex", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (fieldInfo != null)
        {
            int currentPlatformCount = (int)fieldInfo.GetValue(platformSpawner);
            
            // 检查是否到达目标平台数量
            if (currentPlatformCount >= targetPlatformCount)
            {
                // 执行终点逻辑
                GenerateEndPoint();
            }
        }
    }

    /// <summary>
    /// 生成终点 - 在第20个平台位置生成cube并停止平台生成
    /// </summary>
    void GenerateEndPoint()
    {
        if (endPointGenerated) return;

        // 标记终点已生成，防止重复执行
        endPointGenerated = true;

        Debug.Log("finalplatformManager: 到达第20个平台，停止生成并显示终点cube");

        // 停止平台生成器
        if (platformSpawner != null)
        {
            platformSpawner.enabled = false;
            Debug.Log("finalplatformManager: 已停止PlatformSpawner");
        }

        // 如果设置了cubeObject，在第20个平台位置显示或生成cube
        if (cubeObject != null)
        {
            // 获取圆柱体中心位置
            Vector3 centerPos = platformSpawner.cylinderCenter != null 
                ? platformSpawner.cylinderCenter.position 
                : Vector3.zero;

            // 计算第20个平台的位置（与PlatformSpawner相同的螺旋公式）
            int targetIndex = targetPlatformCount - 1; // 因为索引从0开始
            float angle = Mathf.Deg2Rad * targetIndex * platformSpawner.rotationPerPlatform;
            float x = centerPos.x + platformSpawner.spiralRadius * Mathf.Cos(angle);
            float z = centerPos.z + platformSpawner.spiralRadius * Mathf.Sin(angle);
            float y = centerPos.y + platformSpawner.heightPerPlatform * targetIndex;

            // 基础位置 + 手动偏移
            Vector3 cubePosition = new Vector3(x + positionOffset.x, y + positionOffset.y, z + positionOffset.z);

            if (isSceneObject)
            {
                // 如果是场景中的对象：移动到目标位置并显示
                cubeObject.transform.position = cubePosition;
                cubeObject.SetActive(true);
                Debug.Log($"finalplatformManager: 场景中的cube已移动到位置 ({x:F2}, {y:F2}, {z:F2}) 并显示");
            }
            else
            {
                // 如果是预制体：Instantiate创建
                GameObject endCube = Instantiate(cubeObject, cubePosition, Quaternion.identity);
                endCube.name = "EndCube";
                Debug.Log($"finalplatformManager: 在位置 ({x:F2}, {y:F2}, {z:F2}) 生成终点cube");
            }
        }
        else
        {
            Debug.LogWarning("finalplatformManager: cubeObject未设置，跳过cube生成");
        }
    }
}