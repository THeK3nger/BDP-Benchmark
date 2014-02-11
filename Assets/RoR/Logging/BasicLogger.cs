using UnityEngine;
using System.Collections;

namespace RoomOfRequirement.Logger {

    public class BasicLogger {

        private string tag;
        
        public BasicLogger(string tag) {
            this.tag = tag;
        }

        public void Log(string message) {
            BasicLogger.Log(tag, message);
        }

        public void LogError(string message) {
            BasicLogger.LogError(tag, message);
        }

        public void LogWarning(string message) {
            BasicLogger.LogWarning(tag, message);
        }

        public static void Log(string tag, string message) {
            Debug.Log(string.Format("[{0}]: {1}", tag, message));
        }

        public static void LogError(string tag, string message) {
            Debug.LogError(string.Format("[{0}]: {1}", tag, message));
        }

        public static void LogWarning(string tag, string message) {
            Debug.LogWarning(string.Format("[{0}]: {1}", tag, message));
        }
    }

}
