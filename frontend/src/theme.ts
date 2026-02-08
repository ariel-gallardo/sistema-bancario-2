import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#37b7c3',
    },
    secondary: {
      main: '#ffb547',
    },
    background: {
      default: '#050816',
      paper: '#0b1023',
    },
    text: {
      primary: '#f4f6fb',
      secondary: '#9ca7c7',
    },
  },
  typography: {
    fontFamily: '"Space Grotesk", "DM Sans", sans-serif',
    h3: {
      fontWeight: 600,
    },
    h5: {
      fontWeight: 600,
      letterSpacing: 0.5,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 18,
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'linear-gradient(135deg, #111735 0%, #0b1023 100%)',
          border: '1px solid rgba(255,255,255,0.05)',
        },
      },
    },
  },
});

export default theme;
