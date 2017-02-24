// Copyright (C) 2016 Andrea Esuli
// http://www.esuli.it
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace Esuli.Scheggia.Text
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    [Serializable]
    public class TextFile
    {
        public static IEnumerable<TextFile> ReadFile(string filename)
        {
            var iswiki = false;
            using (var stream = new StreamReader(filename))
            {
                var line = stream.ReadLine();
                if (line != null)
                {
                    if (line.StartsWith("<doc id="))
                    {
                        iswiki = true;
                    }
                }
                if (iswiki)
                {
                    var title = "";
                    var url = "";
                    var text = new StringBuilder();
                    do
                    {
                        if (line.StartsWith("<doc id="))
                        {
                            title = Regex.Match(line, "title=\\\"(.*?)\\\"").Groups[1].Value;
                            url = Regex.Match(line, "url=\\\"(.*?)\\\"").Groups[1].Value;
                        }
                        if (line.StartsWith("</doc"))
                        {
                            yield return new TextFile(title, url, text.ToString());
                            text.Clear();
                        }
                        else
                        {
                            text.Append(line);
                            text.Append(" ");
                        }
                    }
                    while ((line = stream.ReadLine()) != null);
                }
            }
            if(!iswiki)
            {
                yield return TextFileFromFile(filename);
            }
        }

        public static TextFile TextFileFromFile(string filename)
        {
            return new TextFile(new FileInfo(filename).Name, filename, File.ReadAllText(filename));
        }

        private string name;
        private string url;
        private string text;

        public TextFile(string name, string url, string text)
        {
            this.name = name;
            this.url = url;
            this.text = text;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string URL
        {
            get
            {
                return url;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }
    }
}
