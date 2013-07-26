<?php
/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

// You can configure the paths here. As default use songs/ and backgrounds/ subdirectories where the script is running.
$ROOT_PATH = dirname(__FILE__);
$SONGS = $ROOT_PATH . '/songs/';
$BACKGROUNDS = $ROOT_PATH . '/backgrounds/';

$ALLOWED_IMAGE_EXT = array(".png" => "image/png", ".jpg" => "image/jpeg", ".jpeg" => "image/jpeg");
$ALLOWED_VIDEO_EXT = array(".mp4" => "video/mp4", ".wmv" => "video/x-ms-wmv", ".avi" => "video/avi", ".mov" => "video/quicktime", ".ogv" => "video/ogg");

error_reporting(0);

function write_log($msg)
{
	// Logging is disabled
	//$fh = @fopen('log.txt', 'a');
	//if ($fh !== FALSE)
	//{
	//	fwrite($fh, $msg."\n");
	//	fclose($fh);
	//}
}

if (!isset($_GET['module']))
	die('This is WordsLive Media server running on PHP');

if ($_GET['module'] == 'backgrounds')
{
	if (!isset($_GET['path']) || $_GET['path'] == '/')
		$path = '';
	elseif (strpos('/' . $_GET['path']. '/', '/../') !== FALSE)
		die('INVALID PATH'); // This can only be reached by requesting index.php directly
	else
		$path = utf8_decode($_GET['path']);
	
	if (isset($_GET['listall'])) // LIST ALL BACKGROUNDS
	{
		// 200 OK
		header('Content-type: text/plain;charset=utf-8');
		listBackgroundDirectories($BACKGROUNDS, true);
	}
	elseif (isset($_GET['list'])) // LIST BACKGROUND DIRECTORY
	{
		// 200 OK
		header('Content-type: text/plain;charset=utf-8');
		
		if (file_exists($BACKGROUNDS . $path) && is_dir($BACKGROUNDS . $path))
		{
			listBackgroundDirectories($BACKGROUNDS . $path, false);
		}
		else
		{
			header("HTTP/1.0 404 Not Found");
			die("Directory not found.");
		}
	}
	else // GET BACKGROUND
	{
		$file = $BACKGROUNDS . $path;
		if (!file_exists($file) || is_dir($file))
		{
			header("HTTP/1.0 404 Not Found");
			die("FILE NOT FOUND");
		}
		$pi = pathinfo($file);
		$ext = '.' . strtolower($pi['extension']);
		
		if (array_key_exists($ext, $ALLOWED_VIDEO_EXT))
			header('Content-Type: '.$ALLOWED_VIDEO_EXT[$ext]);
		elseif (array_key_exists($ext, $ALLOWED_IMAGE_EXT))
			header('Content-Type: '.$ALLOWED_IMAGE_EXT[$ext]);
		else
		{
			header("HTTP/1.0 404 Not Found");
			die("FILE NOT SUPPORTED");
		}
		
		if (isset($_GET['preview']))
		{
			$NEWSIZE = 300;
			
			// get embedded thumbnail
			$thumb = exif_thumbnail($file);
			
			if ($thumb !== FALSE)
			{
				// 200 OK
				header('Content-type: image/jpeg');
				print $thumb;
			}
			else
			{
		
				if($ext == '.jpg' || $ext == '.jpeg')
					$image = ImageCreateFromJPEG($file); 
				elseif($ext=="png"||$ext=="PNG")
					$image = ImageCreateFromPNG($file);
				else
				{
					header("HTTP/1.0 404 Not Found");
					die("NO PREVIEW AVAILABLE");
				}
				
				$w = imagesx($image);
				$h = imagesy($image);
				
				if($w >= $h)
				{
					$wn = $NEWSIZE;
					$ratio = $w/$wn;
					$hn = $h/$ratio;
				}
				elseif($h > $w)
				{  
					$hn = $NEWSIZE;
					$ratio = $h/$hn;
					$wn = $w/$ratio;
				}
				
				$wn = round($wn);
				$hn = round($hn);
	   
				$newImage = ImageCreateTrueColor($wn, $hn); 
				ImageCopyResampled($newImage, $image, 0,0,0,0, $wn, $hn, $w, $h);
				// 200 OK
				header('Content-type: image/jpeg');
				imagejpeg($newImage);
			}
		}
		else
		{
			// 200 OK
			readfile($file);
		}
	}
}
elseif ($_GET['module'] == 'songs')
{
	if (isset($_GET['list'])) // LIST SONGS
	{
		// 200 OK
		header('Content-type: application/json');
		$songs = array();
		
		foreach(glob($SONGS."*") as $file)  
		{
			if (is_dir($file)) // ignore subdirectories
				continue;
			
			$ext = getExtension($file);
			
			if ($ext == '.ppl')
			{
				$data = new SongData($file);
				$songs[] = $data;
			}
		}
		
		echo json_encode($songs);
	}
	elseif (isset($_GET['count'])) // COUNT SONGS
	{
		// 200 OK
		header('Content-type: text/plain');
		$song_count = 0;
		
		foreach(glob($SONGS."*") as $file)  
		{
			if (is_dir($file)) // ignore subdirectories
				continue;
			
			$ext = getExtension($file);
			
			if ($ext == '.ppl')
			{
				$song_count++;
			}
		}
		
		echo $song_count;
	}
	elseif (isset($_GET['filter'])) // FILTER SONGS
	{
		if (!isset($_GET['query']))
			die("NO FILTER QUERY"); // This can only be reached by requesting index.php directly
		else
			$query = $_GET['query'];
			
		$filter = $_GET['filter'];
		
		if (!($filter == 'text' || $filter == 'title' || $filter == 'source' || $filter == 'copyright'))
			die("UNSUPPORTED FILTER METHOD");
			
		if ($filter == 'text' || $filter == 'title')
			$query = SongData::normalizeSearchString($query); // normalize for title and text filters
			
		header('Content-type: application/json');
		$songs = array();
			
		foreach(glob($SONGS."*") as $file)
		{
			if (is_dir($file)) // ignore subdirectories
				continue;
			
			$ext = getExtension($file);
			
			if ($ext == '.ppl')
			{
				$data = new SongData($file);
				if (($filter == 'title' || $filter == 'text') && mb_stripos($data->getSearchTitle(), $query, 0, 'UTF-8') !== FALSE)
					$songs[] = $data;
				elseif ($filter == 'text' && mb_stripos($data->getSearchText(), $query, 0, 'UTF-8') !== FALSE)
					$songs[] = $data;
				elseif ($filter == 'source' && mb_stripos($data->Sources, $query, 0, 'UTF-8') !== FALSE)
					$songs[] = $data;
				elseif ($filter == 'copyright' && mb_stripos($data->Copyright, $query, 0, 'UTF-8') !== FALSE)
					$songs[] = $data;
			}
		}
		
		echo json_encode($songs);
	}
	else // GET/PUT/DELETE SONG
	{
		if (!isset($_GET['path']) || strpos($_GET['path'], '/') !== FALSE || substr($_GET['path'], -4) != '.ppl')
		{
			header("HTTP/1.0 404 Not Found");
			die('INVALID PATH');
		}
		else
		{
			// reconstruct requested path from original REQUEST_URI
			// (for else we won't be able to discern '+' and space)
			$lastslash = strrpos($_SERVER['REQUEST_URI'], '/');
			$file = rawurldecode(substr($_SERVER['REQUEST_URI'], $lastslash + 1));
			$file = $SONGS . utf8_decode($file);
		}
			
		if ($_SERVER['REQUEST_METHOD'] == 'PUT')
		{
			write_log('PUT SONG '.$file);
			if (copy('php://input', $file))
			{
				// 200 OK
				echo "OK";
			}
			else
			{
				header("HTTP/1.0 500 Internal Server Error");
				echo "ERROR";
			}
		}
		elseif ($_SERVER['REQUEST_METHOD'] == 'DELETE')
		{
			write_log('DELETE SONG '.$file);
			
			if (!file_exists($file))
			{
				header("HTTP/1.0 404 Not Found");
				die("FILE NOT FOUND");
			}

			if (unlink($file))
			{
				// 200 OK
				echo "OK";
			}
			else
			{
				header("HTTP/1.0 500 Internal Server Error");
				echo "ERROR";
			}
		}
		elseif ($_SERVER['REQUEST_METHOD'] == 'GET')
		{
			write_log('GET SONG '.$file);
			
			if (!file_exists($file))
			{
				header("HTTP/1.0 404 Not Found");
				die("FILE NOT FOUND");
			}
			else
			{
				$last_modified = gmdate('D, d M Y H:i:s', filemtime($file)) . ' GMT';
				// 200 OK
				header("Content-type: text/xml");
				header("Last-Modified: $last_modified");
				readfile($file);
			}
		}
		else
		{
			header("HTTP/1.0 405 Method Not Allowed");
			die("UNSUPPORTED METHOD");
		}
	}
}

function listBackgroundDirectories($parent, $recursive)
{
	global $BACKGROUNDS, $ALLOWED_VIDEO_EXT, $ALLOWED_IMAGE_EXT;
	
	if (substr($parent, -1) != '/')
		$parent .= '/';
	
	$files = glob($parent."*");
	
	if ($files === false)
	{
		return;
	}
	
	foreach($files as $file)  
	{
		$path = substr($file, strlen($BACKGROUNDS));
		
		if (substr($path, 0, 12) != '[Thumbnails]')
		{
			if (is_dir($file))
			{
				$path .= "/";
			}
			else
			{
				$ext = getExtension($file);
				if (!array_key_exists($ext, $ALLOWED_VIDEO_EXT) && !array_key_exists($ext, $ALLOWED_IMAGE_EXT))
					continue;
			}
			
			echo '/' . utf8_encode($path) . "\n";  
			if (is_dir($file) && $recursive)
				listBackgroundDirectories($file . "/", $recursive);
		}
	}
}

function getExtension($file)
{
	$pi = pathinfo($file);
	$ext = '.' . strtolower($pi['extension']);
	return $ext;
}

class SongData
{
	public $Title;
	public $Filename;
	public $Text;
	public $Copyright;
	public $Sources;
	public $Language;
	
	public function SongData($file)
	{
		global $SONGS;
		$this->Filename = utf8_encode(substr($file, strlen($SONGS)));
		$xml = simplexml_load_file($file);
		
		$result = $xml->xpath('/ppl/general/title');
		$this->Title = (string)$result[0];
		$result = $xml->xpath('/ppl/general/language');
		$this->Language = (string)$result[0];
		$this->Copyright = implode(' ', $xml->xpath('/ppl/information/copyright/text/line'));
		$this->Sources = implode('; ', $xml->xpath('/ppl/information/source/text/line'));
		$result = array_map("SongData::removeChordsFromLine" , $xml->xpath('/ppl/songtext/part/slide/line'));
		$this->Text = implode("\n", $result);
	}
	
	public static function normalizeSearchString($str)
	{
		$str = preg_replace("/(['‚‘’„“”›‹»«])+/", "", $str);
		return preg_replace("/(['’,!.:-]|\\s)+/", " ", $str);
	}
	
	public function getSearchTitle()
	{
		return SongData::normalizeSearchString($this->Title);
	}
	
	public function getSearchText()
	{
		return SongData::normalizeSearchString($this->Text);
	}
	
	// This is a combination of the methods Chords.GetChords() and Chords.ReplaceChords() ported to PHP
	public static function removeChordsFromLine($line)
	{
		$rest = $line;
		
		$pos = 0;
		$start = 0;
		$result = "";
		
		while (($i = strpos($rest, '[')) !== FALSE)
		{			
			$end = strpos($rest, ']', $i);
			if ($end === FALSE)
				break;

			$next = strpos($rest, '[', $i + 1);
			if ($next !== FALSE && $next < $end)
			{
				$rest = substr($rest, $i + 1);
				$pos += $i + 1;
				continue;
			}
			
			$found = true;

			$pos += $i; // absolute position of '[Chord]'
			$chordlen = $end - ($i + 1) + 2; // length of '[Chord]'
			
			$result .= substr($line, $start, $pos - $start);
			$start = $pos + $chordlen;

			$rest = substr($rest, $end + 1);
			$pos += -$i + $end + 1;
		}
		
		$result .= substr($line, $start);
		
		return $result;
	}
}
?>