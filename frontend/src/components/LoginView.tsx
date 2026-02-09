import { type FormEvent } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  Container,
  Grid,
  IconButton,
  InputAdornment,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded';
import SecurityRoundedIcon from '@mui/icons-material/SecurityRounded';
import LockRoundedIcon from '@mui/icons-material/LockRounded';
import { demoUsers } from '../config/api';

interface LoginViewProps {
  formState: { username: string; password: string; clienteId: string };
  onUsernameChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onClienteIdChange: (value: string) => void;
  showPassword: boolean;
  onToggleShowPassword: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  isLoading: boolean;
  error?: string;
}

const LoginView = ({
  formState,
  onUsernameChange,
  onPasswordChange,
  onClienteIdChange,
  showPassword,
  onToggleShowPassword,
  onSubmit,
  isLoading,
  error,
}: LoginViewProps) => (
  <Container maxWidth="xl" className="login-container">
    <Grid container className="login-grid">
      <Grid item xs={12} md={5} className="login-left-panel">
        <Box className="security-panel">
          <Box className="robot-icon">
            <SecurityRoundedIcon sx={{ fontSize: 120, color: '#37b7c3' }} />
          </Box>
          <Typography variant="h4" className="security-title" gutterBottom>
            TE AYUDAMOS
          </Typography>
          <Typography variant="h4" className="security-title">
            A PREVENIR ESTAFAS
          </Typography>
          <Stack spacing={2} mt={4} className="security-tips">
            <Typography variant="body1">• No compartas usuario ni clave con terceros.</Typography>
            <Typography variant="body1">• Validá que la URL sea la oficial antes de operar.</Typography>
            <Typography variant="body1">• Sospechá de llamadas o correos que solicitan datos sensibles.</Typography>
          </Stack>
          <Box mt={5} className="security-footer">
            <Typography variant="body2" className="security-contact">
              Ante cualquier duda comunicate al <strong>11-6842-3330</strong> (24 hs).
            </Typography>
          </Box>
        </Box>
      </Grid>

      <Grid item xs={12} md={7} className="login-right-panel">
        <Paper elevation={0} className="login-form-paper">
          <Box className="logo-header">
            <LockRoundedIcon sx={{ fontSize: 48, color: '#37b7c3', mb: 2 }} />
            <Typography variant="h4" gutterBottom className="login-title">
              Acceso al sistema bancario
            </Typography>
          </Box>

          <Stack component="form" spacing={3} onSubmit={onSubmit} className="login-form">
            <TextField
              required
              label="Usuario"
              value={formState.username}
              onChange={(event) => onUsernameChange(event.target.value)}
              fullWidth
            />

            <TextField
              required
              label="Clave"
              type={showPassword ? 'text' : 'password'}
              value={formState.password}
              onChange={(event) => onPasswordChange(event.target.value)}
              fullWidth
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={onToggleShowPassword} edge="end">
                      {showPassword ? <VisibilityOffRoundedIcon /> : <VisibilityRoundedIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <TextField
              label="Cliente (opcional)"
              placeholder="1 o 2"
              value={formState.clienteId}
              onChange={(event) => onClienteIdChange(event.target.value)}
              helperText="Lo usamos para pedir el resumen en ClienteCuentaService"
              fullWidth
            />

            <Button
              size="large"
              type="submit"
              variant="contained"
              disabled={isLoading}
              className="login-button"
              fullWidth
            >
              {isLoading ? 'Validando...' : 'Ingresar'}
            </Button>

            {error && <Alert severity="error">{error}</Alert>}

            <Box className="demo-credentials">
              <Typography variant="caption" color="text.secondary" gutterBottom>
                Usuarios demo (click para completar el formulario)
              </Typography>
              <Stack spacing={1}>
                {demoUsers.map((user) => (
                  <Stack
                    key={user.username}
                    direction={{ xs: 'column', sm: 'row' }}
                    spacing={1}
                    alignItems={{ xs: 'flex-start', sm: 'center' }}
                  >
                    <Chip
                      label={`${user.username} · ${user.description}`}
                      size="small"
                      color={user.role === 'admin' ? 'primary' : 'info'}
                      onClick={() => {
                        onUsernameChange(user.username);
                        onPasswordChange(user.password);
                        onClienteIdChange(user.clienteId ? String(user.clienteId) : '');
                      }}
                    />
                    {user.clienteId && <Chip label={`Cliente ${user.clienteId}`} size="small" variant="outlined" />}
                  </Stack>
                ))}
              </Stack>
            </Box>
          </Stack>
        </Paper>
      </Grid>
    </Grid>
  </Container>
);

export default LoginView;
