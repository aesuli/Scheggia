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
namespace Esuli.Scheggia.Text.Search
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Search;
    using Esuli.Scheggia.Text.Core;

    public class TextQueryParser<Tcomparer>
        : IQueryParser<string>
        where Tcomparer : IComparer<string>
    {
        public static readonly string fieldWordSeparator = ":";
        public static readonly string negationFlag = "-";

        private char[] separator;
        private string defaultFieldName;

        public TextQueryParser(string defaultFieldName)
        {
            separator = new char[] { ' ' };
            this.defaultFieldName = defaultFieldName;
        }

        public IQuery Parse(string query)
        {
            string[] tokens = query.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (tokens[0].Equals("OR"))
            {
                List<IQuery<PositionHit>> arguments = new List<IQuery<PositionHit>>();
                for (int i = 1; i < tokens.Length; ++i)
                {
                    var queryArgument = ParseToken(tokens[i]);
                    if (queryArgument != null)
                    {
                        arguments.Add(queryArgument);
                    }
                }
                return new OrQuery<PositionHit>(arguments.ToArray());
            }
            else if (tokens[0].Equals("PH"))
            {
                List<IQuery<PositionHit>> arguments = new List<IQuery<PositionHit>>();
                for (int i = 1; i < tokens.Length; ++i)
                {
                    var queryArgument = ParseToken(tokens[i]);
                    if (queryArgument != null)
                    {
                        arguments.Add(queryArgument);
                    }

                }
                return new SequenceQuery<PositionHit>(arguments.ToArray());
            }
            else if (tokens[0].Equals("PR"))
            {
                int maxDistance = int.Parse(tokens[1]);
                List<IQuery<PositionHit>> arguments = new List<IQuery<PositionHit>>();
                for (int i = 2; i < tokens.Length; ++i)
                {
                    var queryArgument = ParseToken(tokens[i]);
                    if (queryArgument != null)
                    {
                        arguments.Add(queryArgument);
                    }
                }
                return new ProximityQuery<PositionHit>(arguments.ToArray(), maxDistance);
            }
            else
            {
                List<IQuery<PositionHit>> arguments = new List<IQuery<PositionHit>>();
                List<IQuery> notArguments = new List<IQuery>();
                foreach (string token in tokens)
                {
                    if (token.Contains(fieldWordSeparator + negationFlag) | (!token.Contains(fieldWordSeparator) && token.StartsWith(negationFlag)))
                    {
                        string cleantoken;
                        if (token.StartsWith(negationFlag))
                        {
                            cleantoken = token.Substring(1);
                        }
                        else
                        {
                            cleantoken = token.Replace(fieldWordSeparator + negationFlag, fieldWordSeparator);
                        }
                        var queryArgument = ParseToken(cleantoken);
                        if (queryArgument != null)
                        {
                            notArguments.Add(queryArgument);
                        }
                    }
                    else
                    {
                        var queryArgument = ParseToken(token);
                        if (queryArgument != null)
                        {
                            arguments.Add(queryArgument);
                        }
                    }
                }
                return new AndQuery<PositionHit>(arguments.ToArray(), notArguments.ToArray());
            }
        }

        private IQuery<PositionHit> ParseToken(string token)
        {
            int fieldWordSeparatorPosition = token.IndexOf(fieldWordSeparator);
            string field;
            string word;
            if (fieldWordSeparatorPosition > 0)
            {
                field = token.Substring(0, fieldWordSeparatorPosition);
                word = token.Substring(fieldWordSeparatorPosition + 1);
            }
            else
            {
                field = defaultFieldName;
                word = token;
            }
            if (word.Length > 0)
            {
                if (word.EndsWith(PrefixQuery<Tcomparer, PositionHit>.prefixFlag))
                {
                    return new PrefixQuery<Tcomparer, PositionHit>(field, word.Substring(0, word.Length - 1));
                }
                else
                {
                    return new SimpleQuery<string, Tcomparer, PositionHit>(field, word);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
