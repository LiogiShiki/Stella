#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} fs_in;

uniform vec3 viewPos;
uniform bool blinn;

// lights
uniform vec3 lightDirections[4];
uniform vec3 lightStrengths[4];
// Shadow
uniform sampler2D shadowMap;

float ShadowCalculation(vec4 fragPosLightSpace, int i)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // calculate bias (based on depth map resolution and slope)
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightDir = normalize(-lightDirections[i]);
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
    // check whether current frag pos is in shadow
    // float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return shadow;
}


void main()
{           
    vec3 color = vec3(0.7,0.7,0.7);
    // ambient
    vec3 ambient = 0.05 * color;


     for(int i = 0; i < 4; ++i) 
     {
        // diffuse
        vec3 lightDir = normalize(-lightDirections[i]);
        vec3 normal = normalize(fs_in.Normal);
        float diff = max(dot(lightDir, normal), 0.0);
        vec3 diffuse = diff * color * lightStrengths[i] /200;
        // specular
        vec3 viewDir = normalize(viewPos - fs_in.FragPos);
        vec3 reflectDir = reflect(-lightDir, normal);
        float spec = 0.0;
        if(blinn)
        {
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
        }
        else
        {
            vec3 reflectDir = reflect(-lightDir, normal);
            spec = pow(max(dot(viewDir, reflectDir), 0.0), 8.0);
        }
        vec3 specular = vec3(0.3) * spec * lightStrengths[i] /250; // assuming bright white light color
        FragColor += vec4(diffuse + specular, 1.0);
     }
    // calculate shadow
    float shadow = ShadowCalculation(fs_in.FragPosLightSpace,0);                      
    FragColor += vec4(ambient, 1.0);
    FragColor *= (1.0 - shadow);

    // HDR tonemapping
    FragColor = FragColor / (FragColor + vec4(1.0));

    // Debug
   // float s = texture(shadowMap,fs_in.TexCoords).r;
   // FragColor = vec4(vec3(s),1.0);


}