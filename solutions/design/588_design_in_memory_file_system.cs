using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

// LC 588 — Design In-Memory File System
//
// API (called by the scratch harness — keep these exact signatures/names):
//   FileSystem()
//   IList<string> Ls(string path)
//   void Mkdir(string path)
//   void AddContentToFile(string filePath, string content)
//   string ReadContentFromFile(string filePath)
//
// Notes:
//   * ls(path): if path is a FILE, return a list with just that file's name;
//     if it's a DIRECTORY, return the immediate children (files + dirs) in
//     LEXICOGRAPHIC order.
//   * mkdir(path): create the directory and any missing intermediate dirs.
//   * addContentToFile(path): create the file if absent, then APPEND content.
//   * readContentFromFile(path): return the file's full content.
//   * All paths are absolute and start with '/'. Root is "/".
public class FileSystem
{
    private class Node
    {
        public string name, content, type;
        public SortedDictionary<string, Node> neighbors;

        public Node(string n, string c, string t)
        {
            this.name = n;
            this.content = c;
            this.type = t;
            neighbors = new();
        }

        public bool addNeighbor(Node n)
        {
            return neighbors.TryAdd(n.name, n);
        }

        public List<string> GetNeighbors()
        {
            return neighbors.Keys.ToList();
        }
    }

    Node root;

    public FileSystem()
    {
        root = new Node("", "", "dir");
    }

    public IList<string> Ls(string path)
    {
        var node = Walk(path);

        if(node.type == "file") return new List<string>{node.name};

        return node.GetNeighbors();
    }

    public void Mkdir(string path)
    {
        AddPath(path, "dir");
    }

    public void AddContentToFile(string filePath, string content)
    {
        AddPath(filePath, "file", content);
    }

    public string ReadContentFromFile(string filePath)
    {
        var node = Walk(filePath);
        return node.content;
    }

    private Node Walk(string path)
    {
        if(path.Length == 1) return root;

        var tokens = path.Split('/');

        var node = root;
        for(int i = 1; i < tokens.Length; ++i)
        {
            node = node.neighbors[tokens[i]];
        }

        return node;
    }

    private void AddPath(string path, string type, string content = "")
    {
        if(path.Length == 1) return;

        var tokens = path.Split('/');

        var node = root;
        for(int i = 1; i < tokens.Length - 1; ++i)
        {
            if(node.neighbors.ContainsKey(tokens[i])) 
            {
                node = node.neighbors[tokens[i]];
            }
            else
            {
                var newNode = new Node(tokens[i], "", "dir");
                node.neighbors.Add(newNode.name, newNode);
                node = node.neighbors[newNode.name];            
            } 
        }

        if(type == "file")
        {
            if(!node.neighbors.ContainsKey(tokens.Last()))
            {
                var newNode = new Node(tokens.Last(), content, "file");
                node.neighbors.Add(newNode.name, newNode);
            }
            else
            {
                node.neighbors[tokens.Last()].content += content;
            }
        }
        else
        {
            var newNode = new Node(tokens.Last(), "", "dir");
            node.neighbors.Add(newNode.name, newNode);
        }
    }
}
