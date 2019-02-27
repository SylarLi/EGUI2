using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EGUI
{
    public sealed class PersistenceTest
    {
        [Persistence]
        private class SimpleClass
        {
            public const int cccc = 1;
            public static string str = "asgab";

            [PersistentField]
            public byte x;
            [PersistentField]
            public int y;
            [PersistentField]
            public string z;
            [PersistentField]
            public TestEnum e;

            public byte xx;
            public int yy;
            public string zz;

            public readonly int rx = 111;
        }

        [Persistence]
        private struct SimpleStruct
        {
            public const int cccc = 1;
            public static string str = "asgab";

            [PersistentField]
            public byte x;
            [PersistentField]
            public int y;
            [PersistentField]
            public string z;
            [PersistentField]
            public TestEnum e;

            public byte xx;
            public int yy;
            public string zz;
        }

        [Persistence]
        private class RefClass
        {
            [PersistentField]
            public SimpleClass simpleClass;
            public SimpleClass nullSimpleClass;

            [PersistentField]
            public SimpleStruct simpleStruct;
            public SimpleStruct defaultSimpleStruct;
        }

        private class RecurRefClass
        {
            public int x;

            public RecurRefClass self;
        }

        private class ComplexRecurRefClassAB
        {
            public int ab;

            public ComplexRecurRefClassA classA;

            public ComplexRecurRefClassB classB;
        }

        private class ComplexRecurRefClassA
        {
            public int a;

            public ComplexRecurRefClassAB classAB;
        }

        private class ComplexRecurRefClassB
        {
            public int b;

            public ComplexRecurRefClassA classA;
        }

        private class BaseClass
        {
            public int x;

            private int y { get; set; }
        }

        private class SubClass : BaseClass
        {
            public int xx;

            protected int yy { get; set; }
        }

        private class AbstractFieldClass
        {
            public BaseClass baseClass;
        }

        private enum TestEnum
        {
            A = 1,
            B = 3,
        }

        private interface TestInterface
        {
            int a { get; set; }
        }

        [Persistence]
        private class ImpInterface : TestInterface
        {
            [PersistentField]
            public int a { get; set; }

            public int b { get; set; }

            public int[] array = new int[0];

            [PersistentField]
            public int this[int index]
            {
                get
                {
                    return array[index];
                }
                set
                {
                    array[index] = value;
                }
            }
        }

        private class TestString
        {
            public string str = "123";

            public TestString() { }
        }

        //private class TestCommandList
        //{
        //    public List<UndoableCommand> stack;
        //}

        public static void Test()
        {
            var persistence = new Persistence();

            //var commandList = new TestCommandList();
            //commandList.stack = new List<UndoableCommand>()
            //{
            //    new UpdateMemberCommand(new Node(), "localAngle", 10),
            //};
            //persistence.Deserialize<UpdateMemberCommand>(persistence.Serialize(new UpdateMemberCommand(new Node(), "localAngle", 10)));

            byte valueByte = 16;
            Debug.Assert(persistence.Deserialize<byte>(persistence.Serialize(valueByte)) == valueByte);

            char valueChar = 'c';
            Debug.Assert(persistence.Deserialize<char>(persistence.Serialize(valueChar)) == valueChar);

            short valueShort = 16;
            Debug.Assert(persistence.Deserialize<short>(persistence.Serialize(valueShort)) == valueShort);

            ushort valueUshort = 16;
            Debug.Assert(persistence.Deserialize<ushort>(persistence.Serialize(valueUshort)) == valueUshort);

            int valueInt = 16;
            Debug.Assert(persistence.Deserialize<int>(persistence.Serialize(valueInt)) == valueInt);

            uint valueUint = 16;
            Debug.Assert(persistence.Deserialize<uint>(persistence.Serialize(valueUint)) == valueUint);

            long valueLong = 16;
            Debug.Assert(persistence.Deserialize<long>(persistence.Serialize(valueLong)) == valueLong);

            ulong valueUlong = 16;
            Debug.Assert(persistence.Deserialize<ulong>(persistence.Serialize(valueUlong)) == valueUlong);

            float valueFloat = 16;
            Debug.Assert(persistence.Deserialize<float>(persistence.Serialize(valueFloat)) == valueFloat);

            double valueDouble = 16;
            Debug.Assert(persistence.Deserialize<double>(persistence.Serialize(valueDouble)) == valueDouble);

            decimal valueDecimal = 16;
            Debug.Assert(persistence.Deserialize<decimal>(persistence.Serialize(valueDecimal)) == valueDecimal);

            string refString = "发电房大是大非sdfasdfasdfasdf";
            Debug.Assert(persistence.Deserialize<string>(persistence.Serialize(refString)) == refString);

            TestEnum enumA = TestEnum.A;
            Debug.Assert(persistence.Deserialize<TestEnum>(persistence.Serialize(enumA)) == enumA);

            var intArray = new int[4]{ 1, 2, 3, 4 };
            var intArrayD = persistence.Deserialize<int[]>(persistence.Serialize(intArray));
            Debug.Assert(intArrayD.Length == intArray.Length);
            for (int i = 0; i < intArrayD.Length; i++)
                Debug.Assert(intArrayD[i] == intArrayD[i]);

            var stringArray = new string[] { "dddd", "abc", "eeadf", "cxvbdfasdf" };
            var stringArrayD = persistence.Deserialize<string[]>(persistence.Serialize(stringArray));
            Debug.Assert(stringArrayD.Length == stringArray.Length);
            for (int i = 0; i < stringArrayD.Length; i++)
                Debug.Assert(stringArrayD[i] == stringArray[i]);

            TestInterface testInterface = new ImpInterface();
            testInterface.a = 1;
            (testInterface as ImpInterface).b = 2;
            var testInterfaceD = persistence.Deserialize<ImpInterface>(persistence.Serialize(testInterface));
            Debug.Assert(testInterfaceD.a == testInterface.a);
            Debug.Assert(testInterfaceD.b != (testInterface as ImpInterface).b);

            //persistence.Register(typeof(SimpleClass));
            var refSimpleClass = new SimpleClass();
            SimpleClass.str = "abcdefg";
            refSimpleClass.x = 1;
            refSimpleClass.y = 2;
            refSimpleClass.z = "1";
            refSimpleClass.e = TestEnum.B;
            refSimpleClass.xx = 3;
            refSimpleClass.yy = 4;
            refSimpleClass.zz = "2";
            var refSimpleClassD = persistence.Deserialize<SimpleClass>(persistence.Serialize(refSimpleClass));
            Debug.Assert(refSimpleClass.x == refSimpleClassD.x);
            Debug.Assert(refSimpleClass.y == refSimpleClassD.y);
            Debug.Assert(refSimpleClass.z == refSimpleClassD.z);
            Debug.Assert(refSimpleClass.e == refSimpleClassD.e);
            Debug.Assert(refSimpleClass.xx != refSimpleClassD.xx);
            Debug.Assert(refSimpleClass.yy != refSimpleClassD.yy);
            Debug.Assert(refSimpleClass.zz != refSimpleClassD.zz);

            var simpleClassList = new List<SimpleClass>();
            simpleClassList.Add(new SimpleClass()
            {
                x = 1,
                y = 2,
                z = "abc",
                xx = 11,
                yy = 22,
                zz = "edf",
            });
            var simpleClassListD = persistence.Deserialize<List<SimpleClass>>(persistence.Serialize(simpleClassList));
            Debug.Assert(simpleClassListD.Count == simpleClassList.Count);
            Debug.Assert(simpleClassListD[0].x == simpleClassList[0].x);
            Debug.Assert(simpleClassListD[0].y == simpleClassList[0].y);
            Debug.Assert(simpleClassListD[0].z == simpleClassList[0].z);
            Debug.Assert(simpleClassListD[0].xx != simpleClassList[0].xx);
            Debug.Assert(simpleClassListD[0].yy != simpleClassList[0].yy);
            Debug.Assert(simpleClassListD[0].zz != simpleClassList[0].zz);

            //persistence.Register(typeof(SimpleStruct));
            var refSimpleStruct = new SimpleStruct();
            refSimpleStruct.x = 1;
            refSimpleStruct.y = 2;
            refSimpleStruct.z = "1";
            refSimpleStruct.e = TestEnum.B;
            refSimpleStruct.xx = 3;
            refSimpleStruct.yy = 4;
            refSimpleStruct.zz = "2";
            var refSimpleStructD = persistence.Deserialize<SimpleStruct>(persistence.Serialize(refSimpleStruct));
            Debug.Assert(refSimpleStruct.x == refSimpleStructD.x);
            Debug.Assert(refSimpleStruct.y == refSimpleStructD.y);
            Debug.Assert(refSimpleStruct.z == refSimpleStructD.z);
            Debug.Assert(refSimpleStruct.e == refSimpleStructD.e);
            Debug.Assert(refSimpleStruct.xx != refSimpleStructD.xx);
            Debug.Assert(refSimpleStruct.yy != refSimpleStructD.yy);
            Debug.Assert(refSimpleStruct.zz != refSimpleStructD.zz);

            //persistence.Register(typeof(RefClass));
            var refClass = new RefClass();
            refClass.simpleClass = refSimpleClass;
            refClass.nullSimpleClass = refSimpleClass;
            refClass.simpleStruct = refSimpleStruct;
            refClass.defaultSimpleStruct = refSimpleStruct;
            var refClassD = persistence.Deserialize<RefClass>(persistence.Serialize(refClass));
            Debug.Assert(refClass.simpleClass.x == refClassD.simpleClass.x);
            Debug.Assert(refClass.simpleClass.y == refClassD.simpleClass.y);
            Debug.Assert(refClass.simpleClass.z == refClassD.simpleClass.z);
            Debug.Assert(refClass.simpleClass.xx != refClassD.simpleClass.xx);
            Debug.Assert(refClass.simpleClass.yy != refClassD.simpleClass.yy);
            Debug.Assert(refClass.simpleClass.zz != refClassD.simpleClass.zz);
            Debug.Assert(refClassD.nullSimpleClass == null);
            Debug.Assert(refClass.simpleStruct.x == refClassD.simpleStruct.x);
            Debug.Assert(refClass.simpleStruct.y == refClassD.simpleStruct.y);
            Debug.Assert(refClass.simpleStruct.z == refClassD.simpleStruct.z);
            Debug.Assert(refClass.simpleStruct.xx != refClassD.simpleStruct.xx);
            Debug.Assert(refClass.simpleStruct.yy != refClassD.simpleStruct.yy);
            Debug.Assert(refClass.simpleStruct.zz != refClassD.simpleStruct.zz);
            Debug.Assert(refClass.defaultSimpleStruct.x != refClassD.defaultSimpleStruct.x);
            Debug.Assert(refClass.defaultSimpleStruct.y != refClassD.defaultSimpleStruct.y);
            Debug.Assert(refClass.defaultSimpleStruct.z != refClassD.defaultSimpleStruct.z);
            Debug.Assert(refClass.defaultSimpleStruct.xx != refClassD.defaultSimpleStruct.xx);
            Debug.Assert(refClass.defaultSimpleStruct.yy != refClassD.defaultSimpleStruct.yy);
            Debug.Assert(refClass.defaultSimpleStruct.zz != refClassD.defaultSimpleStruct.zz);

            //persistence.Register(typeof(RecurRefClass));
            var recurRefClass = new RecurRefClass();
            recurRefClass.x = 10;
            recurRefClass.self = recurRefClass;
            var recurRefClassD = persistence.Deserialize<RecurRefClass>(persistence.Serialize(recurRefClass));
            Debug.Assert(recurRefClassD.x == recurRefClass.x);
            Debug.Assert(recurRefClassD.self == recurRefClassD);

            //persistence.Register(typeof(ComplexRecurRefClassA));
            //persistence.Register(typeof(ComplexRecurRefClassB));
            //persistence.Register(typeof(ComplexRecurRefClassAB));
            var complexRecurRefClassA = new ComplexRecurRefClassA();
            complexRecurRefClassA.a = 11;
            var complexRecurRefClassB = new ComplexRecurRefClassB();
            complexRecurRefClassB.b = 12;
            var complexRecurRefClassAB = new ComplexRecurRefClassAB();
            complexRecurRefClassAB.ab = 13;
            complexRecurRefClassA.classAB = complexRecurRefClassAB;
            complexRecurRefClassB.classA = complexRecurRefClassA;
            complexRecurRefClassAB.classA = complexRecurRefClassA;
            complexRecurRefClassAB.classB = complexRecurRefClassB;
            var complexRecurRefClassABD = persistence.Deserialize<ComplexRecurRefClassAB>(persistence.Serialize(complexRecurRefClassAB));
            Debug.Assert(complexRecurRefClassABD.ab == complexRecurRefClassAB.ab);
            Debug.Assert(complexRecurRefClassABD.classA.a == complexRecurRefClassAB.classA.a);
            Debug.Assert(complexRecurRefClassABD.classB.b == complexRecurRefClassAB.classB.b);
            Debug.Assert(complexRecurRefClassABD.classA == complexRecurRefClassABD.classB.classA);
            Debug.Assert(complexRecurRefClassABD == complexRecurRefClassABD.classA.classAB);

            //persistence.Register(typeof(BaseClass));
            //persistence.Register(typeof(SubClass));
            //persistence.Register(typeof(AbstractFieldClass));
            var abstractFieldClass = new AbstractFieldClass();
            abstractFieldClass.baseClass = new SubClass() { x = 1, xx = 2 };
            var abstractFieldClassD = persistence.Deserialize<AbstractFieldClass>(persistence.Serialize(abstractFieldClass));
            Debug.Assert(abstractFieldClassD.baseClass.x == 1);
            Debug.Assert(abstractFieldClassD.baseClass is SubClass);
            Debug.Assert((abstractFieldClassD.baseClass as SubClass).xx == 2);

            var baseClassList = new List<BaseClass>();
            baseClassList.Add(new SubClass()
            {
                xx = 123,
                x = 1,
            });
            baseClassList.Add(new BaseClass()
            {
                x = 234,
            });
            var baseClassListD = persistence.Deserialize<List<BaseClass>>(persistence.Serialize(baseClassList));
            Debug.Assert(baseClassListD.Count == baseClassList.Count);
            Debug.Assert(baseClassListD[0].x == baseClassList[0].x);
            Debug.Assert((baseClassListD[0] as SubClass).xx == (baseClassList[0] as SubClass).xx);
            Debug.Assert(baseClassListD[1].x == baseClassList[1].x);

            var complexStructure = new Dictionary<List<ComplexRecurRefClassAB>, Dictionary<ComplexRecurRefClassA, ComplexRecurRefClassB>>();
            var refAB = new ComplexRecurRefClassAB();
            refAB.ab = 13;
            var refA = new ComplexRecurRefClassA();
            refA.a = 11;
            var refB = new ComplexRecurRefClassB();
            refB.b = 12;
            refAB.classA = refA;
            refAB.classB = refB;
            refA.classAB = refAB;
            refB.classA = refA;
            var key1 = new List<ComplexRecurRefClassAB>();
            key1.Add(refAB);
            key1.Add(refAB);
            var val1 = new Dictionary<ComplexRecurRefClassA, ComplexRecurRefClassB>();
            val1.Add(refA, refB);
            complexStructure.Add(key1, val1);
            var complexStructureD = persistence.Deserialize<Dictionary<List<ComplexRecurRefClassAB>, Dictionary<ComplexRecurRefClassA, ComplexRecurRefClassB>>>(persistence.Serialize(complexStructure));
            Debug.Assert(complexStructureD.Count == complexStructure.Count);
            var arrayKeys = complexStructure.Keys.ToArray();
            var arrayKeysD = complexStructureD.Keys.ToArray();
            Debug.Assert(arrayKeysD[0].Count == arrayKeys[0].Count);
            Debug.Assert(arrayKeysD[0][0] == arrayKeysD[0][1]);
            for (int j = 0; j < arrayKeys[0].Count; j++)
            {
                Debug.Assert(arrayKeysD[0][j].ab == arrayKeys[0][j].ab);
            }
            var arrayVals = complexStructure.Values.ToArray();
            var arrayValsD = complexStructureD.Values.ToArray();
            Debug.Assert(arrayValsD[0].Count == arrayVals[0].Count);
            Debug.Assert(arrayValsD[0].Keys.ToArray()[0].a == arrayVals[0].Keys.ToArray()[0].a);
            Debug.Assert(arrayValsD[0].Values.ToArray()[0].b == arrayVals[0].Values.ToArray()[0].b);
            Debug.Assert(arrayValsD[0].Keys.ToArray()[0] == arrayKeysD[0][0].classA);
            Debug.Assert(arrayValsD[0].Values.ToArray()[0] == arrayKeysD[0][0].classB);

            var vector4 = new Vector4(1, 2, 3, 4);
            var vector4D = persistence.Deserialize<Vector4>(persistence.Serialize(vector4));
            Debug.Assert(vector4 == vector4D);

            var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            var colorD = persistence.Deserialize<Color>(persistence.Serialize(color));
            Debug.Assert(color == colorD);

            var quaternion = Quaternion.Euler(100, 200, 300);
            var quaternionD = persistence.Deserialize<Quaternion>(persistence.Serialize(quaternion));
            Debug.Assert(quaternion == quaternionD);

            //var guiStyle = GUI.skin.button;
            //var guiStyleD = persistence.Deserialize<GUIStyle>(persistence.Serialize(guiStyle));
            //Debug.Assert(guiStyle.clipping == guiStyleD.clipping);
            //Debug.Assert(guiStyle.border.left == guiStyleD.border.left);
            //Debug.Assert(guiStyle.margin.right == guiStyleD.margin.right);
            //Debug.Assert(guiStyle.padding.top == guiStyleD.padding.top);
            //Debug.Assert(guiStyle.contentOffset == guiStyleD.contentOffset);
            //Debug.Assert(guiStyle.stretchWidth == guiStyleD.stretchWidth);
            //Debug.Assert(guiStyle.stretchHeight == guiStyleD.stretchHeight);
            //Debug.Assert(guiStyle.normal.background == guiStyleD.normal.background);
            //Debug.Assert(guiStyle.normal.textColor == guiStyleD.normal.textColor);
            //if (guiStyle.normal.scaledBackgrounds.Length > 0)
            //    Debug.Assert(guiStyle.normal.scaledBackgrounds[0] == guiStyleD.normal.scaledBackgrounds[0]);
            //Debug.Assert(guiStyle.onNormal.background == guiStyleD.onNormal.background);
            //Debug.Assert(guiStyle.onNormal.textColor == guiStyleD.onNormal.textColor);
            //if (guiStyle.onNormal.scaledBackgrounds.Length > 0)
            //    Debug.Assert(guiStyle.onNormal.scaledBackgrounds[0] == guiStyleD.onNormal.scaledBackgrounds[0]);
            //Debug.Assert(guiStyle.imagePosition == guiStyleD.imagePosition);
            //Debug.Assert(guiStyle.font == guiStyleD.font);

            //var eguiButton = new Button();
            //eguiButton.name = "abc";
            //var eguiButtonD = persistence.Deserialize<Button>(persistence.Serialize(eguiButton));
            //Debug.Assert(eguiButton.name == eguiButtonD.name);

            var subxField = typeof(SubClass).GetField("x", BindingFlags.Instance | BindingFlags.Public);
            var subxFieldD = persistence.Deserialize<FieldInfo>(persistence.Serialize(subxField));
            Debug.Assert(subxField.FieldType == subxFieldD.FieldType);
            Debug.Assert(subxField.ReflectedType == subxFieldD.ReflectedType);
            Debug.Assert(subxField.Name == subxFieldD.Name);

            var subyProperty = typeof(SubClass).GetProperty("yy", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var subyPropertyD = persistence.Deserialize<PropertyInfo>(persistence.Serialize(subyProperty));
            Debug.Assert(subyProperty.PropertyType == subyPropertyD.PropertyType);
            Debug.Assert(subyProperty.ReflectedType == subyPropertyD.ReflectedType);
            Debug.Assert(subyProperty.Name == subyPropertyD.Name);
        }
    }
}
