<!doctype html>
<html>
<head></head>
<body style="color: black; text-align: center; font-size: large;  background-image: url('https://pixabay.com/get/55e4d6444f56a914f6d1867dda6d49214b6ac3e45656744c732a7fd496/nature-3437545_1920.jpg');">
	<div >
	<?php
		$xml = simplexml_load_file('WeatherData.xml') or die("Error: Cannot connect to database");
		echo '<h1>Weather info no.'.$xml['No'].'</h1>';
		echo '<h2>Latest Data</h2>';
		echo '<h3>Date:'.$xml->now['date'].' Time:'.$xml->now['time'].'</h3>';
		//echo '<h4>';
		echo 'Description: '.$xml->now->description.'<br>';
		echo 'Detailed research date: '.$xml->now->dateInfo->day.' '.$xml->now->dateInfo->month.' '.$xml->now->dateInfo->year.'<br>';
		echo 'Basic research date: '.$xml->now->dateInfo->fullDate.'<br>';
		echo 'Time: '.$xml->now->dateInfo->time.'<br>';
		echo 'Temperature: '.$xml->now->data->temperature.'<br>';
		echo 'Pressure: '.$xml->now->data->pressure.'<br>';
		echo 'Approximatd Height: '.$xml->now->data->appHeight.'<br>';
		echo 'Light: '.$xml->now->data->light.'<br>';
		echo 'Device name: '.$xml->now->data->machineName.'<br>';
		//echo '</h4>';
		if ($xml->then->description != "<NoFilesAvilable>") {
			echo '<h2>Previous Data</h2>';
			echo '<h3>Date:'.$xml->then['date'].' Time:'.$xml->then['time'].'</h3>';
			echo 'Description: '.$xml->then->description.'<br>';
			echo 'Basic research date: '.$xml->then->dateInfo->fullDate.'<br>';
			echo 'Time: '.$xml->then->dateInfo->time.'<br>';
			echo 'Temperature: '.$xml->then->data->temperature.'<br>';
			echo 'Pressure: '.$xml->then->data->pressure.'<br>';
			echo 'Approximatd Height: '.$xml->then->data->appHeight.'<br>';
			echo 'Light: '.$xml->then->data->light.'<br>';
			echo 'Device name: '.$xml->then->data->machineName.'<br>';
		} 					
	?>
	</div>
</body>
</html>