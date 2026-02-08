import { type FormEvent, useMemo, useState, useEffect } from 'react';
import { Box, Container, Grid, Stack } from '@mui/material';
import { Navigate, Route, Routes } from 'react-router-dom';
import './App.css';
import { useAppDispatch, useAppSelector } from './hooks';
import { login, logout, setClienteId } from './features/auth/authSlice';
import { fetchDashboardData, resetDashboard } from './features/dashboard/dashboardSlice';
import { demoCredentials } from './config/api';
import { formatCurrency, formatDate } from './utils/formatters';
import LoginView from './components/LoginView';
import DashboardHeader from './components/DashboardHeader';
import SessionCard from './components/SessionCard';
import AccountCard from './components/AccountCard';
import MovementsCard from './components/MovementsCard';

const App = () => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector((state) => state.auth);
  const dashboard = useAppSelector((state) => state.dashboard);

  const [formState, setFormState] = useState({
    username: demoCredentials.username,
    password: demoCredentials.password,
    clienteId: demoCredentials.clienteId ? String(demoCredentials.clienteId) : '',
  });
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const parsedCliente = formState.clienteId.trim().length ? Number(formState.clienteId) : null;
    const clienteIdValue = parsedCliente && !Number.isNaN(parsedCliente) ? parsedCliente : null;
    dispatch(
      login({
        username: formState.username,
        password: formState.password,
        clienteId: clienteIdValue,
      }),
    );
  };

  useEffect(() => {
    if (auth.token && auth.clienteId) {
      void dispatch(fetchDashboardData(auth.clienteId));
    }
  }, [auth.token, auth.clienteId, dispatch]);

  const handleLogout = () => {
    dispatch(logout());
    dispatch(resetDashboard());
  };

  const handleChangeCliente = (clienteId: number | null) => {
    dispatch(setClienteId(clienteId));
    if (clienteId && auth.token) {
      void dispatch(fetchDashboardData(clienteId));
    }
  };

  const handleRefreshDashboard = () => {
    if (auth.clienteId) {
      void dispatch(fetchDashboardData(auth.clienteId));
    }
  };

  const isAdmin = useMemo(() => auth.roles.some((role) => role.toLowerCase().includes('admin')), [auth.roles]);

  return (
    <Box className="app-shell">
      <div className="aurora-glow" aria-hidden />
      <Routes>
        <Route path="/" element={<Navigate to={auth.token ? '/dashboard' : '/login'} replace />} />
        <Route
          path="/login"
          element={
            auth.token ? (
              <Navigate to="/dashboard" replace />
            ) : (
              <LoginView
                formState={formState}
                onUsernameChange={(value) => setFormState((prev) => ({ ...prev, username: value }))}
                onPasswordChange={(value) => setFormState((prev) => ({ ...prev, password: value }))}
                onClienteIdChange={(value) => setFormState((prev) => ({ ...prev, clienteId: value }))}
                showPassword={showPassword}
                onToggleShowPassword={() => setShowPassword((prev) => !prev)}
                onSubmit={handleSubmit}
                isLoading={auth.status === 'loading'}
                error={auth.error}
              />
            )
          }
        />
        <Route
          path="/dashboard"
          element={
            auth.token ? (
              <Container maxWidth="lg">
                <Stack spacing={4} className="dashboard-container">
                  <DashboardHeader
                    username={auth.username}
                    clienteId={auth.clienteId}
                    onLogout={handleLogout}
                    isAdmin={isAdmin}
                    onChangeClienteId={handleChangeCliente}
                  />

                  <SessionCard
                    username={auth.username}
                    roles={auth.roles}
                    clienteId={auth.clienteId}
                    expiresAt={auth.expiresAt}
                  />

                  <Grid container spacing={3}>
                    <Grid item xs={12} md={7}>
                      <AccountCard
                        isLoading={dashboard.status === 'loading'}
                        resumen={dashboard.resumen}
                        clienteId={auth.clienteId}
                        onRefresh={handleRefreshDashboard}
                        formatCurrency={formatCurrency}
                      />
                    </Grid>

                    <Grid item xs={12} md={5}>
                      <MovementsCard
                        isLoading={dashboard.status === 'loading'}
                        movimientos={dashboard.movimientos}
                        tarjeta={dashboard.tarjeta}
                        error={dashboard.error}
                        formatCurrency={formatCurrency}
                        formatDate={formatDate}
                      />
                    </Grid>
                  </Grid>
                </Stack>
              </Container>
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
        <Route path="*" element={<Navigate to={auth.token ? '/dashboard' : '/login'} replace />} />
      </Routes>
    </Box>
  );
};
export default App;
