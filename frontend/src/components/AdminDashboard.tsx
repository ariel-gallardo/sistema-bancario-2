import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Container,
  Divider,
  Grid,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Paper,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded';
import AssignmentIndRoundedIcon from '@mui/icons-material/AssignmentIndRounded';
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded';
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';
import DoneRoundedIcon from '@mui/icons-material/DoneRounded';
import PendingActionsRoundedIcon from '@mui/icons-material/PendingActionsRounded';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { apiConfig } from '../config/api';
import { useAppSelector } from '../hooks';
import type { AdminOverviewResponse, ClienteRegistroSummary } from '../types/api';

const sidebarSections = [
  { id: 'overview', label: 'Resumen ejecutivo', icon: <DashboardRoundedIcon /> },
  { id: 'registros', label: 'Solicitudes de alta', icon: <AssignmentIndRoundedIcon /> },
  { id: 'movimientos', label: 'Panel de movimientos', icon: <CreditCardRoundedIcon /> },
] as const;

type SidebarSection = (typeof sidebarSections)[number]['id'];

const AdminDashboard = () => {
  const navigate = useNavigate();
  const auth = useAppSelector((state) => state.auth);
  const token = auth.token;
  const [selectedSection, setSelectedSection] = useState<SidebarSection>('overview');

  const [overview, setOverview] = useState<AdminOverviewResponse | null>(null);
  const [overviewStatus, setOverviewStatus] = useState<'idle' | 'loading' | 'error'>('idle');
  const [overviewError, setOverviewError] = useState<string>();

  const [pending, setPending] = useState<ClienteRegistroSummary[]>([]);
  const [pendingStatus, setPendingStatus] = useState<'idle' | 'loading' | 'error'>('idle');
  const [pendingError, setPendingError] = useState<string>();

  const headers = useMemo(() => (token ? { Authorization: `Bearer ${token}` } : undefined), [token]);

  const fetchOverview = useCallback(async () => {
    if (!headers) {
      setOverviewStatus('error');
      setOverviewError('No encontramos la sesión del administrador.');
      return;
    }

    setOverviewStatus('loading');
    setOverviewError(undefined);
    try {
      const response = await axios.get<AdminOverviewResponse>(`${apiConfig.clientes}/api/admin/overview`, { headers });
      setOverview(response.data);
      setOverviewStatus('idle');
    } catch (error) {
      setOverviewStatus('error');
      setOverviewError(extractErrorMessage(error, 'No pudimos cargar el resumen.'));
    }
  }, [headers]);

  const fetchPending = useCallback(async () => {
    if (!headers) {
      setPendingStatus('error');
      setPendingError('No encontramos la sesión del administrador.');
      return;
    }

    setPendingStatus('loading');
    setPendingError(undefined);
    try {
      const response = await axios.get<ClienteRegistroSummary[]>(`${apiConfig.clientes}/api/admin/registros/pendientes`, {
        headers,
      });
      setPending(response.data);
      setPendingStatus('idle');
    } catch (error) {
      setPendingStatus('error');
      setPendingError(extractErrorMessage(error, 'No pudimos cargar las solicitudes.'));
    }
  }, [headers]);

  const handleMarkReviewed = async (registroId: string) => {
    if (!headers) {
      return;
    }

    try {
      await axios.post(
        `${apiConfig.clientes}/api/admin/registros/${registroId}/revisar`,
        null,
        { headers },
      );
      setPending((prev) => prev.filter((registro) => registro.registroId !== registroId));
    } catch (error) {
      setPendingError(extractErrorMessage(error, 'No pudimos actualizar la solicitud.'));
    }
  };

  useEffect(() => {
    if (!token) {
      return;
    }

    if (selectedSection === 'overview') {
      void fetchOverview();
    } else if (selectedSection === 'registros') {
      void fetchPending();
    } else if (selectedSection === 'movimientos') {
      navigate('/admin/movimientos');
    }
  }, [fetchOverview, fetchPending, navigate, selectedSection, token]);

  const renderOverview = () => (
    <Stack spacing={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5">Indicadores principales</Typography>
        <Tooltip title="Actualizar">
          <span>
            <IconButton onClick={() => fetchOverview()} disabled={overviewStatus === 'loading'}>
              <RefreshRoundedIcon />
            </IconButton>
          </span>
        </Tooltip>
      </Stack>

      {overviewStatus === 'loading' && (
        <Box className="admin-loading-panel">
          <CircularProgress color="inherit" />
        </Box>
      )}

      {overviewStatus === 'error' && overviewError && <Alert severity="error">{overviewError}</Alert>}

      {overview && overviewStatus !== 'loading' && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Paper className="admin-metric-card" elevation={3}>
              <Typography variant="overline" color="text.secondary">
                Clientes activos
              </Typography>
              <Typography variant="h3">{overview.totalClientes}</Typography>
              <Typography variant="body2" color="text.secondary">
                Clientes autenticados en la plataforma
              </Typography>
            </Paper>
          </Grid>
          <Grid item xs={12} md={4}>
            <Paper className="admin-metric-card" elevation={3}>
              <Stack direction="row" alignItems="center" spacing={1}>
                <PendingActionsRoundedIcon color="warning" />
                <Typography variant="overline" color="text.secondary">
                  Solicitudes pendientes
                </Typography>
              </Stack>
              <Typography variant="h3">{overview.solicitudesPendientes}</Typography>
              <Typography variant="body2" color="text.secondary">
                Última solicitud: {overview.ultimaSolicitudUtc ? formatDateTime(overview.ultimaSolicitudUtc) : 'N/D'}
              </Typography>
            </Paper>
          </Grid>
          <Grid item xs={12} md={4}>
            <Paper className="admin-metric-card" elevation={3}>
              <Stack direction="row" alignItems="center" spacing={1}>
                <DoneRoundedIcon color="success" />
                <Typography variant="overline" color="text.secondary">
                  Solicitudes revisadas
                </Typography>
              </Stack>
              <Typography variant="h3">{overview.solicitudesRevisadas}</Typography>
              <Typography variant="body2" color="text.secondary">
                Incluye registros migrados manualmente
              </Typography>
            </Paper>
          </Grid>
        </Grid>
      )}
    </Stack>
  );

  const renderPending = () => (
    <Stack spacing={3}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="h5">Solicitudes pendientes</Typography>
        <Tooltip title="Actualizar">
          <span>
            <IconButton onClick={() => fetchPending()} disabled={pendingStatus === 'loading'}>
              <RefreshRoundedIcon />
            </IconButton>
          </span>
        </Tooltip>
      </Stack>

      {pendingStatus === 'loading' && (
        <Box className="admin-loading-panel">
          <CircularProgress color="inherit" />
        </Box>
      )}

      {pendingStatus === 'error' && pendingError && <Alert severity="error">{pendingError}</Alert>}

      {pendingStatus !== 'loading' && pending.length === 0 && (
        <Alert severity="success">No hay solicitudes nuevas para revisar.</Alert>
      )}

      {pending.length > 0 && (
        <Stack spacing={2}>
          {pending.map((registro) => (
            <Paper key={registro.registroId} className="admin-registro-card" elevation={2}>
              <Stack spacing={1}>
                <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }}>
                  <Box>
                    <Typography variant="subtitle1" fontWeight={600}>
                      {registro.nombreCompleto}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Documento: {registro.documento} · Registrado el {formatDateTime(registro.creadoEnUtc)}
                    </Typography>
                  </Box>
                  <Stack direction="row" spacing={1} mt={{ xs: 1, sm: 0 }}>
                    <Chip label={registro.estado} color="warning" size="small" variant="outlined" />
                    <Chip label={registro.email} size="small" variant="outlined" />
                  </Stack>
                </Stack>
                <Typography variant="body2" color="text.secondary">
                  Contacto: {registro.telefono}
                </Typography>
                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button
                    variant="contained"
                    size="small"
                    onClick={() => handleMarkReviewed(registro.registroId)}
                    disabled={pendingStatus === 'loading'}
                  >
                    Marcar como revisado
                  </Button>
                </Stack>
              </Stack>
            </Paper>
          ))}
        </Stack>
      )}
    </Stack>
  );

  return (
    <Container maxWidth="lg" className="admin-dashboard-container">
      <Grid container spacing={3}>
        <Grid item xs={12} md={3}>
          <Paper className="admin-sidebar" elevation={4}>
            <Stack spacing={2}>
              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Administrador
                </Typography>
                <Typography variant="h5">
                  {auth.profile?.nombre ?? 'Usuario'}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {auth.profile?.email}
                </Typography>
              </Box>
              <Chip label="Rol: Admin" color="secondary" size="small" />
              <Divider />
              <List>
                {sidebarSections.map((section) => (
                  <ListItemButton
                    key={section.id}
                    selected={selectedSection === section.id}
                    onClick={() => setSelectedSection(section.id)}
                  >
                    <ListItemIcon>{section.icon}</ListItemIcon>
                    <ListItemText primary={section.label} />
                  </ListItemButton>
                ))}
              </List>
              <Divider />
              <Stack spacing={1}>
                <Button variant="text" onClick={() => navigate('/dashboard')}>
                  Volver al home banking
                </Button>
                <Button variant="outlined" onClick={() => navigate('/admin/movimientos')}>
                  Ir al panel de movimientos
                </Button>
              </Stack>
            </Stack>
          </Paper>
        </Grid>
        <Grid item xs={12} md={9}>
          <Paper className="admin-content" elevation={4}>
            {selectedSection === 'overview' && renderOverview()}
            {selectedSection === 'registros' && renderPending()}
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

const extractErrorMessage = (error: unknown, fallback: string) => {
  if (axios.isAxiosError(error)) {
    if (typeof error.response?.data?.message === 'string') {
      return error.response.data.message;
    }
    if (typeof error.message === 'string' && error.message.length > 0) {
      return error.message;
    }
  } else if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
};

const formatDateTime = (value: string) => {
  try {
    return new Date(value).toLocaleString();
  } catch (error) {
    return value;
  }
};

export default AdminDashboard;
