using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomListSerialization {
  public class ListNode {

    public ListNode Prev;
    public ListNode Next;

    /// <summary>
    /// Произвольный элемент внутри списка 
    /// </summary>
    public ListNode Rand;

    public string Data;

    public ListNode() {
    }

    public ListNode(string data) {
      Data = data;
    }

    public override string ToString() {
      return Data;
    }
  }

  public class ListRand {
    public static readonly Encoding encoding = Encoding.UTF8;
    public const char endLineSymbol = '\n';
    public const char inlineDelimiter = '\t';

    /// <summary>
    /// Узел, являющийся началом списка
    /// </summary>
    public ListNode Head;

    /// <summary>
    /// Узел, являющийся концом списка
    /// </summary>
    public ListNode Tail;

    /// <summary>
    /// Количество узлов в списке
    /// </summary>
    public int Count;

    public ListRand() {
    }

    public ListRand(ListNode head, ListNode tail, int count) {
      Head = head;
      Tail = tail;
      Count = count;
    }

    private void MapList(Action<ListNode> mapper, ListNode node) {
      if (node == null) {
        return;
      }

      mapper.Invoke(node);

      MapList(mapper, node.Next);
    }

    public void Serialize(FileStream s) {
      s.Write(encoding.GetBytes($"{Count}{endLineSymbol}"));

      Dictionary<ListNode, int> randNodeIndexes = new();

      MapList(node => randNodeIndexes.Add(node, randNodeIndexes.Count), Head);

      MapList(node => {
        int randIndex = (node.Rand == null) ? -1 : randNodeIndexes[node.Rand];

        s.Write(encoding.GetBytes($"{node.Data}{inlineDelimiter}{randIndex}{endLineSymbol}"));
      }, Head);
    }

    public void Deserialize(FileStream s) {
      Dictionary<int, ListNode> indexNodes = new();
      List<byte> allBytes = new();
      int intByte;

      while ((intByte = s.ReadByte()) != -1) {
        allBytes.Add((byte)intByte);
      }

      List<string> allLines = encoding
        .GetString(allBytes.ToArray())
        .Split(endLineSymbol, StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Trim(endLineSymbol))
        .ToList();

      Count = int.Parse(allLines[0]);

      for (int index = 0; index < Count; index++) {
        ListNode node = new(allLines[index + 1]);

        indexNodes.Add(index, node);

        if (index == 0) {
          Head = node;
        }

        if (index == Count - 1) {
          Tail = node;
        }
      }

      for (int index = 0; index < Count; index++) {
        ListNode node = indexNodes[index];

        if (index > 0) {
          node.Prev = indexNodes[index - 1];
        }

        if (index < Count - 1) {
          node.Next = indexNodes[index + 1];
        }

        string[] split = node.Data.Split(inlineDelimiter);

        node.Data = split[0];

        int randIndex = int.Parse(split[1]);

        if (indexNodes.ContainsKey(randIndex)) {
          node.Rand = indexNodes[randIndex];
        }
      }
    }
  }
}
