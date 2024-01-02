function getStorage() {
  if (process.env.NODE_ENV === "development") {
    return window.sessionStorage;
  } else {
    return window.localStorage;
  }
}

export default getStorage;
