/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      boxShadow: {
        custom: "0 5px 50px -10px rgba(0, 0, 0, 0.1)",
      },
    },
    keyframes: {
      fadeIn: {
        "0%": { opacity: 0 },
        "100%": { opacity: 1 },
      },
      slideIn: {
        "0%": { opacity: 0, transform: "translateX(-100%)" },
        "100%": { opacity: 1, transform: "translateX(0)" },
      },
      grow: {
        "0%": { transform: "scale(1)" },
        "100%": { transform: "scale(1.2)" },
      },
      shrink: {
        "0%": { transform: "scale(1.2)" },
        "100%": { transform: "scale(1)" },
      },
      spin: {
        "0%": { transform: "rotate(0deg)" },
        "100%": { transform: "rotate(360deg)" },
      },
    },
    animation: {
      fadeIn: "fadeIn .25s ease-in-out forwards var(--delay, 0)",
      slideIn: "slideIn .25s ease-in-out forwards var(--delay, 0)",
      grow: "grow .5s forwards",
      shrink: "shrink .5s forwards",
      spin: "spin 1s linear infinite",
      none: "none",
    },
  },
  plugins: [],
};
