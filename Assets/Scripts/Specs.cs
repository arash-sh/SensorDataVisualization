using UnityEngine;
using System;

public class Specs {

    #region  Timing 
    public static readonly DateTime StartTime = new DateTime(2018, 5, 13, 0, 0, 0);
    public static readonly DateTime EndTime = new DateTime(2018, 6, 30, 23, 59, 59);
    public static readonly string DateFormat = "yyyy-MM-dd";
    public static readonly string TimeFormat = "HH:mm:ss";
    public static readonly float TimeHourStep = 1F;
    public static readonly int FrameRate = 2;    
    #endregion

    #region Physical Properties
    public static readonly float MoistureLowerBound = 0;
    public static readonly float MoistureUpperBound = 40;
    #endregion

    #region Scaling  
    public static float WorldScale = 0.1F; // scale of the imported 3D model, used in scaled function in utilities
    public static float CameraMotionSpeed = Utilities.Scaled(8F); // step for camera motion
    public static float CameraRotationSpeed = Utilities.Scaled(20F); // step for camera rotation
    #endregion

    #region Configurations  
    public enum VIZ_MODE { TEXTURE, TEXTURE_PAINT, PARTICLE_COLOR, PARTICLE_RADIUS, PARTICLE_LOOSE, TUBES };
    public static VIZ_MODE ThisVizMode = VIZ_MODE.TEXTURE_PAINT;

    public static bool MouseControlsCamera = true;
    #endregion

    #region Ressources (loaded in the utilities)
    public static Material TransparenMat;
    public static GameObject WeatherPrefab;
    public static GameObject SensorIndicatorPrefab;
    public static GameObject LowPolySpherePrefab;
    #endregion

 
}
