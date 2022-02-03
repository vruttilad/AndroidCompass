using Android.App;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Runtime.CompilerServices;

namespace AndroidCompass
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity,ISensorEventListener
    {
     
        private ImageView imageView;
        private float[] mGravity = new float[3], mGeomagnatic = new float[3];
        private float azimuth = 0f;
        private float currectAzimuth = 0f;
        private SensorManager mSensorManager;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            imageView = FindViewById<ImageView>(Resource.Id.compass);
            mSensorManager = (SensorManager)GetSystemService(SensorService);

        }
        protected override void OnPause()
        {
            base.OnPause();
            mSensorManager.UnregisterListener(this);
        }
        protected override void OnResume()
        {
            base.OnResume();
            mSensorManager.RegisterListener(this, mSensorManager.GetDefaultSensor(SensorType.MagneticField), SensorDelay.Game);
            mSensorManager.RegisterListener(this, mSensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Game);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
           
        }

        public void OnSensorChanged(SensorEvent e)
        {
            ProcessCompass(e);
        }
      [MethodImpl(MethodImplOptions.Synchronized)]

        private void ProcessCompass(SensorEvent e)
        {
            float alpha = 0.97f;
            if(e.Sensor.Type == SensorType.Accelerometer)
            {
                mGravity[0] = alpha * mGravity[0] + (1 - alpha) * e.Values[0];
                mGravity[1] = alpha * mGravity[1] + (1 - alpha) * e.Values[1];
                mGravity[2] = alpha * mGravity[2] + (1 - alpha) * e.Values[2];
            }

            if (e.Sensor.Type == SensorType.MagneticField)
            {
                mGeomagnatic[0] = alpha * mGeomagnatic[0] + (1 - alpha) * e.Values[0];
                mGeomagnatic[1] = alpha * mGeomagnatic[1] + (1 - alpha) * e.Values[1];
                mGeomagnatic[2] = alpha * mGeomagnatic[2] + (1 - alpha) * e.Values[2];
            }

            float[] R=new float[9], I = new float[9];
            bool success = SensorManager.GetRotationMatrix(R, I, mGravity, mGeomagnatic);
            if (success)
            {
                float[] orientation = new float[3];
                SensorManager.GetOrientation(R, orientation);
                azimuth = (float)(orientation[0] * (180 / Math.PI));
                azimuth = (azimuth + 360) % 360;

                Android.Views.Animations.Animation anim = new RotateAnimation(-currectAzimuth, azimuth, Android.Views.Animations.Dimension.RelativeToSelf, 0.5f,
                    Android.Views.Animations.Dimension.RelativeToSelf, 0.5f);

                currectAzimuth = azimuth;
                anim.Duration = 500;
                anim.RepeatCount = 0;
                anim.FillAfter = true;

                imageView.StartAnimation(anim);
            }
        }
    }
}