import { useState } from "react";
import getStorage from "../utils/getStorage";

export function useLocalStorage(
  keyName: string,
  defaultValue: string | null,
): [string, (newValue: string) => void] {
  const storage = getStorage();

  const [storedValue, setStoredValue] = useState(() => {
    try {
      const item = storage.getItem(keyName);
      if (item) {
        return JSON.parse(item);
      } else {
        storage.setItem(keyName, JSON.stringify(defaultValue));
        return defaultValue;
      }
    } catch (err) {
      return defaultValue;
    }
  });

  function setValue(newValue: string) {
    try {
      setStoredValue(newValue);
      storage.setItem(keyName, JSON.stringify(newValue));
    } catch (err) {
      console.error(err);
    }
  }

  return [storedValue, setValue];
}
