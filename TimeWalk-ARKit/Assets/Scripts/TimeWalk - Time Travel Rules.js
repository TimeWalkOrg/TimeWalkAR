	var yearBuilt : int;
	var yearReplaced : int;
	private var lastYearDisplayed : int = 0;

function Update () {
	
//    Debug.Log(TimeWalk_Controls.yearNowValue.ToString ()); //TimeWalk_Controls.yearNowValue
	if(lastYearDisplayed != TimeWalk_Controls.yearNowValue){ // if year has changed, then...
		if((TimeWalk_Controls.yearNowValue >= yearBuilt) && (TimeWalk_Controls.yearNowValue < yearReplaced)){ // Object is VISIBLE
			// Child objects are VISIBLE (i.e. SetActive)

 			for (var child : Transform in transform)
 				{
				child.gameObject.SetActive(true);
				}
		}
		else {
			// Child objects are NOT visible

				for (var child : Transform in transform)
 				{
				child.gameObject.SetActive(false);
				}
			}
		lastYearDisplayed = TimeWalk_Controls.yearNowValue; // update lastYearDisplayed to the current TimeWalk date
	}
}