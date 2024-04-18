using System;
using System.Collections.Generic;
using System.Text;
using OpenUtau.Api;
using System.Linq;


namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("French xCV Phonemizer", "FR xCV", "Mim", language: "FR")]
  
    public class FrenchxCVPhonemizer : SyllableBasedPhonemizer {

        private readonly string[] vowels = "ah,ae,eh,ee,oe,ih,oh,oo,ou,uh,en,in,on,oi,ui".Split(",");
        private readonly string[] consonants = "b,d,f,g,j,k,l,m,n,p,r,s,sh,t,v,w,y,z,gn,.,-,R,BR,_hh".Split(",");
        private readonly Dictionary<string, string> dictionaryReplacements = (
            "aa=ah;ai=ae;ei=eh;eu=ee;ee=ee;oe=oe;ii=ih;au=oh;oo=oo;ou=ou;uu=uh;an=en;in=in;un=in;on=on;uy=ui;" +
            "bb=b;dd=d;ff=f;gg=g;jj=j;kk=k;ll=l;mm=m;nn=n;pp=p;rr=r;ss=s;ch=sh;tt=t;vv=v;ww=w;yy=y;zz=z;gn=gn;4=l;hh=h;").Split(';')
                .Select(entry => entry.Split('='))
                .Where(parts => parts.Length == 2)
                .Where(parts => parts[0] != parts[1])
                .ToDictionary(parts => parts[0], parts => parts[1]);


        private string[] plosives = "t,k,g,p,b,d".Split(",");


        protected override string[] GetVowels() => vowels;
        protected override string[] GetConsonants() => consonants;
        protected override string GetDictionaryName() => "cmudict_fr.txt";
        protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => dictionaryReplacements;

        protected override List<string> ProcessSyllable(Syllable syllable) {
            string prevV = syllable.prevV;
            string[] cc = syllable.cc;
            string v = syllable.v;
            var lastC = cc.Length - 1;
            var firstC = 0;

            string basePhoneme;
            var phonemes = new List<string>();

            if (prevV == "ui") {
                prevV = "ih";
            }


            // --------------------------- STARTING V ------------------------------- //
            if (syllable.IsStartingV) {

                // try -V, - V then defaults to V
                basePhoneme = $"- {v}";



                // --------------------------- is VV ------------------------------- //
            } else if (syllable.IsVV) {  // if VV
                if (!CanMakeAliasExtension(syllable)) {

                    basePhoneme = $"{v}";

                } else {
                    // the previous alias will be extended
                    basePhoneme = null;
                }
                // --------------------------- STARTING CV ------------------------------- //
            } else if (syllable.IsStartingCVWithOneConsonant) {

                basePhoneme = $"- {cc[0]}{v}";

                // --------------------------- STARTING CCV ------------------------------- //
            } else if (syllable.IsStartingCVWithMoreThanOneConsonant) {


                basePhoneme = $"- {cc.Last()}{v}";

                for (int i = 0; i < cc.Length - 1; i++) {
                    string ccx = $"{cc[i]} {cc[i + 1]}";

                    if (HasOto(ccx, syllable.tone)) {
                        phonemes.Add(ccx);
                    }
                }
            }

                // --------------------------- IS VCV ------------------------------- //
                else if (syllable.IsVCVWithOneConsonant) {

                basePhoneme = $"{cc[0]}{v}";

            } else {

                phonemes.Add($"{prevV} {cc[0]}");

                basePhoneme = $"{cc[cc.Length - 2]}{cc.Last()}{v}";
                var max = cc.Length - 2;
                if (!HasOto(basePhoneme, syllable.tone)) {
                    max--;
                    basePhoneme = $"- {cc.Last()}{v}";
                }
                for (int i = 0; i < max; i++) {
                    string ccx = $"{cc[i]} {cc[i + 1]}";

                    if (HasOto(ccx, syllable.tone)) {
                        phonemes.Add(ccx);
                    }
                }

            }


            phonemes.Add(basePhoneme);
            return phonemes;
        }
        protected override List<string> ProcessEnding(Ending ending) {
            string[] cc = ending.cc;
            string v = ending.prevV;

            var phonemes = new List<string>();
            bool hasEnding = false;

            // --------------------------- ENDING V ------------------------------- //
            if (ending.IsEndingV) {
                phonemes.Add($"{v} -");

            } else {
                // --------------------------- ENDING VC ------------------------------- //
                if (ending.IsEndingVCWithOneConsonant) {

                    phonemes.Add($"{v} {cc[0]}");
                    if (plosives.Contains(cc[0])) {
                        phonemes.Add($"{cc[0]} -");
                    }
                } else {
                    // --------------------------- ENDING VCC ------------------------------- //
                    phonemes.Add($"{v} {cc[0]}");
                    for (int i = 0; i < cc.Length - 1; i++) {
                        string ccx = $"{cc[i]} {cc[i + 1]}";

                        if (HasOto(ccx, ending.tone)) {
                            phonemes.Add(ccx);
                        }
                    }
                    if (plosives.Contains(cc.Last())) {
                        phonemes.Add($"{cc.Last()} -");
                    }

                }


            }

            // ---------------------------------------------------------------------------------- //

            return phonemes;
        }





    }
}
