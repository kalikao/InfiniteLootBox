using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;

/// <summary>
/// Settings loader.
/// </summary>
/// <remarks>
/// Loads the "Settings.ini" file from StreamingAssets, parses and stores it's values.
/// </remarks>
public class Settings
{
    public int Variety;
    public Rigged[] RiggedItems;
    private int mRigCurID = 0;
    public List<float> Probabilities;
	public float AudioReverbDecay;
	public float AudioReverbVol;
	public float AudioFlangeMix;
    public int OpenParticlesMax;
    public int OpenParticlesRate;
    public int PoofParticlesMax; //particles at the destruction of a Loot3D
    public int PoofParticlesRate;
    public float ChestOpeningSpeed;
    public float AudioSpeed;
    public float ChestSpawnDelay;
    public float ChestDestroyDelay;
    public IEnumerator init(string fname)
    {
        #if UNITY_EDITOR_OSX
            var settings = new WWW("file:///"+Application.streamingAssetsPath + "/" + fname);
        #else
            var settings = new WWW(Application.streamingAssetsPath + "/" + fname);
        #endif        
        yield return settings;
        INIParser ini = new INIParser();
        ini.OpenFromString(settings.text);

        Variety = ini.ReadValue("LootSettings", "Variety", 1);
        OpenParticlesMax = ini.ReadValue("LootSettings", "OpenParticlesMax", 160);
        OpenParticlesRate = ini.ReadValue("LootSettings", "OpenParticlesRate", 555);
        PoofParticlesMax = ini.ReadValue("LootSettings", "PoofParticlesMax", 160);
        PoofParticlesRate = ini.ReadValue("LootSettings", "PoofParticlesRate", 555);
        ChestSpawnDelay = (float)ini.ReadValue("LootSettings", "ChestSpawnDelay", 3.0);
        ChestDestroyDelay = (float)ini.ReadValue("LootSettings", "ChestDestroyDelay", 3.0);
        ChestOpeningSpeed = (float)ini.ReadValue("LootSettings", "OpenSpeed", 1.0);
        AudioSpeed = (float)ini.ReadValue("LootSettings", "AudioSpeed", 1.0);

        var str = ini.ReadValue("LootSettings", "Rarity", "1,1,1,1,1");
        var odds = Regex.Matches(str, @"\d+").OfType<Match>().Select((m) => float.Parse(m.Value)).ToArray();
		float sum = odds.Sum();
		float acc = 0;
		Probabilities = new List<float>();
		foreach(float f in odds){
			acc+=f;
			Probabilities.Add(acc/sum);
			// Debug.Log(Probabilities.Last());
		}
		
        str = ini.ReadValue("LootSettings", "RiggedOpens", "");
        char[] tierChars = {'C','U','R','E','L'};
        RiggedItems = Regex.Matches(str, @"(\w)(\d+)").OfType<Match>().Select(
            (m) => new Rigged(
                        Array.IndexOf(tierChars,m.Groups[1].ToString()[0]),
                        int.Parse(m.Groups[2].ToString())
            )).ToArray();
        // Debug.Log(RiggedItems[0]);
        AudioReverbDecay =    1f*ini.ReadValue("LootSettings", "AudioReverbDecay", 20);
        AudioReverbVol   = -10000+100f*ini.ReadValue("LootSettings", "AudioReverbVol", 90);
        AudioFlangeMix   = 0.01f*ini.ReadValue("LootSettings", "AudioFlangeMix", 0);
        

		// ini.Close();//probably unnecessary		
    }
    public struct Rigged
    {
        public Rigged(int t, int n){
            tier = t;
            number = n;
        }
        public int tier;
        public int number;
        override public string ToString(){
            return tier+":"+number;
        }
    }
}