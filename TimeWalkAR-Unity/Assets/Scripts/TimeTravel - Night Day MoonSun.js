	var nowIsDay : boolean = true;
	var dayMaterial:Material;
	var nightMaterial:Material;
	var colorMoon : Color = Color.blue;
	var colorSun : Color = Color.white;
	var dayIntensity : float = 0.4f;
	var nightIntensity : float = 0.1f;
function Update () {
	if(Input.GetKeyDown(KeyCode.N)){
		if(nowIsDay==true){
			RenderSettings.skybox=nightMaterial;
			RenderSettings.ambientIntensity=nightIntensity;
			GetComponent.<Light>().color = colorMoon;
			nowIsDay = false;
		}
		else {
			RenderSettings.skybox=dayMaterial;
			GetComponent.<Light>().color = colorSun;
			RenderSettings.ambientIntensity=dayIntensity;
			nowIsDay = true;
			}
	}
}