Shader "UI_Occluded"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_OccludedOpacity("Occluded Opacity", Range(0, 1)) = 0.2
	}
	
	SubShader
	{
		
		Tags { "RenderType"="Opaque" }

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		
		Pass
		{
			Name "Renders when occluded"

			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask Off
			Cull Back
			ColorMask RGBA
			ZWrite Off
			ZTest Greater
			Offset 0 , 0


			CGPROGRAM

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct meshData
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct interpolators
			{
				half4 vertex : SV_POSITION;
				half4 UV : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform half4 _Color;
			uniform half _OccludedOpacity;
			
			interpolators vert ( meshData v )
			{
				interpolators o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.UV = v.texcoord;
				o.vertex = UnityObjectToClipPos(v.vertex);


				return o;
			}
			
			fixed4 frag (interpolators i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				fixed4 finalColor;

				finalColor = ( tex2D( _MainTex, i.UV) );
				finalColor *= _Color;
				finalColor.a = _OccludedOpacity;
				return finalColor;
			}
			ENDCG
		}


		Pass
		{
			Name "Renders normally"

			Blend Off
			AlphaToMask Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0


			CGPROGRAM

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				//only defining to not throw compilation error over Unity 5.5
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"


				struct meshData
				{
					float4 vertex : POSITION;
					half4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct interpolators
				{
					half4 vertex : SV_POSITION;
					half4 UV : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				uniform sampler2D _MainTex;
				uniform half4 _Color;


				interpolators vert(meshData v)
				{
					interpolators o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.UV = v.texcoord;
					o.vertex = UnityObjectToClipPos(v.vertex);


					return o;
				}

				fixed4 frag(interpolators i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					fixed4 finalColor;

					finalColor = (tex2D(_MainTex, i.UV));
					finalColor *= _Color;
					return finalColor;
				}
				ENDCG
			}




	}
	
	
}
