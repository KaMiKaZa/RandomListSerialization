using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomListSerialization {
  public class ListNode {
    /// <summary>
    /// Элемент последовательности, который является предыдущим к текущему
    /// </summary>
    /// <remarks>
    /// null указывает, что текущий элемент является первым в последовательности
    /// </remarks>
    public ListNode Prev;

    /// <summary>
    /// Элемент последовательности, который является следующим за текущим
    /// </summary>
    /// <remarks>
    /// null указывает, что текущий элемент является последним в последовательности
    /// </remarks>
    public ListNode Next;

    /// <summary>
    /// Ссылка на произвольный элемент внутри списка 
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

    /// <summary>
    /// Символ, который разделяет узлы между собой во время сериализации
    /// </summary>
    public const char endLineSymbol = '\n';

    /// <summary>
    /// Символ, который разделяет информацию об одном узле на части во время сериализации
    /// </summary>
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

    /// <summary>
    /// Применяет переданную функцию к каждому пройденному узлу
    /// </summary>
    /// <remarks>Выполняет обход по направлению от node к концу списка</remarks>
    /// <param name="mapper">Произвольная функция, которая будет вызвана на каждом пройденном узле</param>
    /// <param name="node">Узел, с которого будет начат обход списка</param>
    private void MapList(Action<ListNode> mapper, ListNode node) {
      if (node == null) {
        return;
      }

      mapper.Invoke(node);

      MapList(mapper, node.Next);
    }

    /// <summary>
    /// Выполняет сериализацию текущего объекта
    /// </summary>
    /// <param name="s">Файловый поток, в который будет записан сериализованный объект</param>
    public void Serialize(FileStream s) {
      s.Write(encoding.GetBytes($"{Count}{endLineSymbol}"));

      // Используется для индексации узлов и последующего сохранения ссылок на Rand в файл
      Dictionary<ListNode, int> randNodeIndexes = new();

      MapList(node => randNodeIndexes.Add(node, randNodeIndexes.Count), Head);

      MapList(node => {
        int randIndex = (node.Rand == null) ? -1 : randNodeIndexes[node.Rand];

        s.Write(encoding.GetBytes($"{node.Data}{inlineDelimiter}{randIndex}{endLineSymbol}"));
      }, Head);
    }

    /// <summary>
    /// Выполняет десериализацию из файла, перезаписывая данные текущего объекта
    /// </summary>
    /// <param name="s">Файловый поток, из которого будет считан сериализованный объект</param>
    public void Deserialize(FileStream s) {
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

      // Используется для индексации узлов и последующего восстановления ссылок на Rand из файла
      Dictionary<int, ListNode> indexNodes = new();

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
