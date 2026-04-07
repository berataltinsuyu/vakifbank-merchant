/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "primary":                   "#705d00",
        "primary-container":         "#ffd700",
        "on-primary-container":      "#705e00",
        "primary-fixed":             "#ffe16d",
        "on-surface":                "#1a1c1c",
        "surface-container-lowest":  "#ffffff",
        "surface-container-low":     "#f3f3f3",
        "surface-container":         "#eeeeee",
        "surface-container-high":    "#e8e8e8",
        "surface-container-highest": "#e2e2e2",
        "outline-variant":           "#d0c6ab",
        "on-surface-variant":        "#4d4732",
        "secondary":                 "#5f5e5e",
        "tertiary":                  "#00696f",
        "error":                     "#ba1a1a",
      },
      fontFamily: {
        headline: ["Manrope", "sans-serif"],
        body:     ["Inter", "sans-serif"],
      },
    },
  },
  plugins: [],
};