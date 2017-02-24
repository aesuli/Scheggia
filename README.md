# Scheggia

Scheggia is a .net search engine.

I wrote Scheggia during the last years, mostly for the pleasure of coding it and personal study and research on the topic.
It makes a heavy use of generic types, which is a design choice I like and I frequently make when I use .net.

It is designed fit well in small/medium-sized applications, e.g., desktop search, single server search.
In fact I have used it in projects of relatively small size:
- to create a text search service for the [CoPhIR](http://cophir.isti.cnr.it/) dataset of 106 million flickr images, indexing title, description and text of each image (this repository includes the code to index that collection);
- to create a search tool on wikipedia dumps (this repository includes the code to index wikipedia dumps preprocessed with [this XML/wiki-markup to text tool](https://github.com/aesuli/wikipedia-extractor))

It implements parallel off-line (on disk) indexing, with efficient index merging.
It supports multiple field indexes, i.e., indexes in which the various parts of the input document are indexed and can be searched/weighted separately.
It implements boolean filters, phrase and proximity search, custom match scoring, all supporting easy extension through interfaces.

Scheggia uses only core .net functionalities, so it runs perfectly also on mono and any platform for which a .net implementation is available.

## ScheggiaText

The ScheggiaText project is a demo app that implements in a single executable all the functionalities of a search engine: index, merge/update, search.

>ScheggiaText.exe
Command: (just hit return to exit)
Usage:
Index:                -i  <number of parallel indexers> <merge way count> <million of hits per temporary index> split|single <[path]indexName> <pathToIndex>+
Search:               -s  <[path]indexName>
Print fields:         -df <[path]indexName>
Print lexicon:        -dl <[path]indexName> <fieldname>
Print posting lists:  -dp <[path]indexName> <fieldname>
Print all hits:       -dh <[path]indexName> <fieldname>
Merge indexes:        -m  <[path]targetIndexName> <[path]indexToMerge>+
Update indexes:       -u  <[path]targetIndexName> <[path]indexToUpdate>+
Append indexes:       -a  <[path]targetIndexName> <[path]indexToAppend>+

### Index

Given a set of textual document in a directory ./data , an index is created by the command:

>ScheggiaText.exe -i 12 3 10 single index data
+(data)..................................................
Input data acquired in xx.xx seconds.

yyy files (zzzzz hits) indexed in kkk.kkkk seconds.
>

where the difference between 'single' and 'split' in the fact the a single file is used to store document and position info for words in 'single', while in 'split' this information is stored in two different files.
The 'single' setup should have a better performance when proximity and phrase are very frequent, whereas 'split' should work better when simple boolean queries are dominant.
Apart from this efficiency aspect, the fact that an index is 'single' or 'split' is completely invisible at search time, and any other index operation (e.g., reading it to merge it with another index).

The content of an index can be inspected with the -df -dl -dp -dh flags:

>ScheggiaText.exe -df index
Index name:     index
MaxId:  11
Fields count:   1
        Field name:     text
        Lexicon type:   System.String
        Hit type:       Esuli.Scheggia.Text.Core.PositionHit
Press enter to exit.

A scheggia index is structured in fields, which acts as a distinct indexing space. 
This feature can be used to separately index, and then search, different parts of documents.
For example, if an index has 'title' and 'body' fields and if the word 'sun' appears only in the 'title' part of the document, that document will be not returned when searching 'sun' on the 'body' field.

Fields may as well overlap, and be combined in search to boost multiple matches.
For example, an indexing of html may define the 'body', 'anchor', and 'bold' fields, and if a word appears in a not bold text anchor in the body of the document then it will be indexes on the 'body' and 'anchor' fields, but not in the 'bold' one.
At search time, a query can be defined to boost a result score if the word appears in multiple fields for the same document.

The demo app ScheggiaText uses a single 'text' field, which is set by selected by default in queries.

#### Inspecting the index

Indexes can be inspected by dumping their lexicon, and even all the posting lists in fine details:

Lexicon:
```
>ScheggiaText.exe -dl index text 
		...
        990     16      acted
        991     35      acting
        992     555     action
        993     1       actionpacked
        994     79      actions
        995     5       activate
        996     5       activated
        997     1       activating
        998     27      active
		...
```

Posting lists:
```
>ScheggiaTextTest.exe -dh index text
		...
        998     27      active  0 (1) [ (8,46) ]  3 (2) [ (6,37) (10,113)]  ... [...]
		...
```

In this last case the number in round parenthesis are the word- and char-offset at which the word 'active' appears in the various indexed documents.

### Merge

### Search

A simple command line search interface is also available:

```
>ScheggiaText.exe -s index
```

By default any command is interpreted as an AND query on the 'text' field

```
>dd
AND ( text:active )
0       1
1       1
2       1
3       1
10      1
11      1
12      1
13      1

8 results of 8 shown
Search time: 0.0002131
>dd ee
AND ( text:very , text:active )
0       2
1       2
2       2
3       2
10      2
11      2
12      2
13      2

8 results of 8 shown
Search time: 0.0003456
```

The result is a list of doc ids, each one with a count of hits in the document and a score.

More details result can be obtained by issuing the 'Dy' (details yes, 'Dn' to turn it off) command:

```
>PH very active
SEQUENCE ( text:very , text:active )
```

The results list the id of the documents, you can get the file name by putting the id in square brackets:

````
>[3]
3       c:\Scheggia\data\testdocD.txt
```

and get the content of the document by prefixing the id with an exclamation mark:

```
>[!3]
3       c:\Scheggia\data\testdocD.txt
-------
Andrea is a very active person.
-------
```
