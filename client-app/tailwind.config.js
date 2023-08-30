/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      gridTemplateColumns: {
        '20': 'repeat(20, minmax(0, 1fr))',
      },
        blur: {
        'custom-sm': '2px',
        'custom-none': '0',
        },
      backgroundColor: {
        'half-opacity': 'rgba(0, 0, 0, 0.2)',
        'default-opacity': 'rgba(0, 0, 0, 1)',
      },
      textColor: {
        'custom-red': 'rgba(255, 0, 0, 0.7)', // 70% opacity red
      },
      boxShadow: {
        'custom': '0 5px 50px -10px rgba(0, 0, 0, 0.1)',
      }
    },
  },
  plugins: [],
}

