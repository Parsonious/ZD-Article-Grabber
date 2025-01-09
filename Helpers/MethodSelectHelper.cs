using System.Reflection;

namespace ZD_Article_Grabber.Helpers
{
    public class MethodSelectHelper(uint Seed, object target, string name)
    {
        private readonly uint _seed = Seed;
        private readonly string _name = name;
        private readonly object _target = target ?? throw new ArgumentNullException(nameof(target));

        public Object? CallMethod()
        {
            byte methodCount = GetClassMethodCount(_target);
            uint methodIndex = GetMethodIndex(methodCount);
            MethodInfo[] methods = GetPublicMethods(_target);
            MethodInfo selectedMethod = SelectMethod(methodIndex, methods);

            if(selectedMethod.ContainsGenericParameters)
            {
                // You must specify the generic type arguments
                Type[] typeArguments = { typeof(string) }; //adjust type as needed here
                selectedMethod = selectedMethod.MakeGenericMethod(typeArguments);
            }

            object[] parameters = { _name }; //all potential method calls take one argument

            return selectedMethod.Invoke(_target, parameters);
        }

        private protected uint GetMethodIndex(byte methodCount)
        {
            if(methodCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(methodCount), "Method count must be greater than 0");
            }
            return _seed % methodCount;
        }

        private protected static byte GetClassMethodCount<T>(T obj)
        {
            return obj == null
                ? throw new ArgumentNullException(nameof(obj)) // Null check
                : (byte)obj.GetType() 
                .GetMethods(BindingFlags.Public | BindingFlags.Instance) // Get all public instance methods
                .Where(method => method.DeclaringType == obj.GetType()) // Exclude inherited methods
                .Count();
        }

        private protected static MethodInfo[] GetPublicMethods(object obj)
        {
            return obj == null
                ? throw new ArgumentNullException(nameof(obj)) // Null check
                : obj.GetType() //if not null, get the type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance) // Get all public instance methods
                .Where(method => method.DeclaringType == obj.GetType()) // Exclude inherited methods
                .ToArray(); // Convert to array
        }

        private protected static MethodInfo SelectMethod(uint methodIndex, MethodInfo[] methods)
        {
            return methods[methodIndex];
        }
    }
}
