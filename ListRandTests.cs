using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomListSerialization {
  public class ListRandTests {
    public const string SerializeFileName = "ListRandSerialize.txt";

    public static List<ListNode> NodeList;
    public static ListRand RandomList;

    [SetUp]
    public void SetUp() {
      NodeList = new();

      Random random = new Random();

      // диапазон [3, ?) гарантирует, что между head и tail будет как минимум один узел
      int randomCount = random.Next(3, 20);

      // создаём двусвязный список узлов
      for (int i = 0; i < randomCount; i++) {
        if (i == 0) {
          NodeList.Add(new ListNode("head"));
        } else {
          // ставим Data в значение "tail" для последнего узла
          NodeList.Add(new ListNode((i == randomCount - 1) ? "tail" : $"node {i}"));

          // связываем текущий узел с предыдущим
          NodeList[i - 1].Next = NodeList[i];
          NodeList[i].Prev = NodeList[i - 1];
        }
      }

      // устанавливаем случайные элементы узлам кроме первого и последнего
      for (int i = 1; i < randomCount - 1; i++) {
        if (random.Next(0, 2) == 1) {
          NodeList[i].Rand = NodeList[random.Next(0, randomCount)];
        }
      }

      RandomList = new ListRand(NodeList[0], NodeList[randomCount - 1], randomCount);
    }

    [Test]
    public void TestSerialize() {
      using (var fileStream = File.OpenWrite(SerializeFileName)) {
        RandomList.Serialize(fileStream);
      }

      var serializedFileInfo = new FileInfo(SerializeFileName);

      // проверяем, что файл с сериализацией существует и он не пустой
      Assert.True(serializedFileInfo.Exists && serializedFileInfo.Length > 0);
    }

    [Test]
    public void TestDeserialize() {
      using (var fileWriteStream = File.OpenWrite(SerializeFileName)) {
        RandomList.Serialize(fileWriteStream);

        // обнуляем объект, который был создан перед запуском теста
        RandomList = new();
      }

      using (var fileReadStream = File.OpenRead(SerializeFileName)) {
        RandomList.Deserialize(fileReadStream);
      }

      // проверяем, что процесс Serialize -> Deserialize не испортил значение Count
      Assert.AreEqual(NodeList.Count, RandomList.Count);

      List<ListNode> deserializedNodes = new();
      ListNode node = RandomList.Head;

      for (int i = 0; i < RandomList.Count; i++) {
        deserializedNodes.Add(node);

        node = node.Next;
      }

      CollectionAssert.AreEqual(
        deserializedNodes.Select(node => $"{node.Data} + {node.Rand?.Data ?? "null"}"),
        NodeList.Select(node => $"{node.Data} + {node.Rand?.Data ?? "null"}")
      );
    }

    [TearDown]
    public void TearDown() {
      NodeList = null;
      RandomList = null;

      if (File.Exists(SerializeFileName)) {
        File.Delete(SerializeFileName);
      }
    }
  }
}
