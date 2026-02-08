import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ChangeEvent } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Container,
  Divider,
  FormControl,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';
import ManageSearchRoundedIcon from '@mui/icons-material/ManageSearchRounded';
import FilterAltRoundedIcon from '@mui/icons-material/FilterAltRounded';
import { useNavigate } from 'react-router-dom';
import { apiConfig } from '../config/api';
import { useAppSelector } from '../hooks';
import type { LogEntry, LogPage } from '../types/api';

type GraphqlResponse<T> = {
  data?: T;
  errors?: { message?: string }[];
};

const LOGS_QUERY = `
  query GetLogs(
    $page: Int!
    $pageSize: Int!
    $serviceName: String
    $severity: String
    $search: String
    $correlationId: String
    $requestPath: String
  ) {
    logs(
      page: $page
      pageSize: $pageSize
      serviceName: $serviceName
      severity: $severity
      search: $search
      correlationId: $correlationId
      requestPath: $requestPath
    ) {
      page
      pageSize
      totalCount
      items {
        id
        timestampUtc
        serviceName
        message
        severity
        requestPath
        stackTrace
        correlationId
        payload
      }
    }
  }
`;

type FiltersState = {
  serviceName: string;
  severity: string;
  search: string;
  correlationId: string;
  requestPath: string;
};

type FilterKey = keyof FiltersState;

const filterLabels: Record<FilterKey, string> = {
  serviceName: 'Servicio',
  severity: 'Severidad',
  search: 'Texto',
  correlationId: 'Correlación',
  requestPath: 'Ruta',
};

const severityOptions = ['Fatal', 'Error', 'Warning', 'Info', 'Debug', 'Trace', 'Success'];

const createDefaultFilters = (): FiltersState => ({
  serviceName: '',
  severity: '',
  search: '',
  correlationId: '',
  requestPath: '',
});

const sanitizeFilters = (filters: FiltersState): FiltersState => ({
  serviceName: filters.serviceName.trim(),
  severity: filters.severity.trim(),
  search: filters.search.trim(),
  correlationId: filters.correlationId.trim(),
  requestPath: filters.requestPath.trim(),
});

const toGraphqlVariable = (value: string) => {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : undefined;
};

const AdminDashboard = () => {
  const navigate = useNavigate();
  const auth = useAppSelector((state) => state.auth);
  const graphqlEndpoint = buildGraphqlEndpoint(apiConfig.logs);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [logPage, setLogPage] = useState<LogPage | null>(null);
  const [status, setStatus] = useState<'idle' | 'loading' | 'error'>('idle');
  const [error, setError] = useState<string>();
  const [filters, setFilters] = useState<FiltersState>(() => createDefaultFilters());
  const [appliedFilters, setAppliedFilters] = useState<FiltersState>(() => createDefaultFilters());

  const updateFilter = (key: FilterKey, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleApplyFilters = () => {
    const normalized = sanitizeFilters(filters);
    setAppliedFilters(normalized);
    setPage(0);
  };

  const handleResetFilters = () => {
    const reset = createDefaultFilters();
    setFilters(reset);
    setAppliedFilters(reset);
    setPage(0);
  };

  const handleRemoveFilter = (key: FilterKey) => {
    setFilters((prev) => ({ ...prev, [key]: '' }));
    setAppliedFilters((prev) => ({ ...prev, [key]: '' }));
    setPage(0);
  };

  const fetchLogs = useCallback(
    async (signal?: AbortSignal) => {
      setStatus('loading');
      setError(undefined);
      try {
        const response = await fetch(graphqlEndpoint, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            query: LOGS_QUERY,
            variables: { page: page + 1, pageSize: rowsPerPage },
          }),
          signal,
        });

        if (!response.ok) {
          throw new Error(`El servicio respondió ${response.status}`);
        }

        const payload = (await response.json()) as GraphqlResponse<{ logs: LogPage }>;
              serviceName: toGraphqlVariable(appliedFilters.serviceName),
              severity: toGraphqlVariable(appliedFilters.severity),
              search: toGraphqlVariable(appliedFilters.search),
              correlationId: toGraphqlVariable(appliedFilters.correlationId),
              requestPath: toGraphqlVariable(appliedFilters.requestPath),

        if (payload.errors?.length) {
          throw new Error(payload.errors.map((err) => err.message ?? 'Error en GraphQL').join(' | '));
        }

        if (!payload.data?.logs) {
          throw new Error('La respuesta de GraphQL llegó vacía.');
        }

        setLogPage(payload.data.logs);
        setStatus('idle');
      } catch (err) {
        if (signal?.aborted || (err instanceof DOMException && err.name === 'AbortError')) {
          return;
        }
        setStatus('error');
        setError(extractGraphqlError(err));
      }
    },
    [graphqlEndpoint, page, rowsPerPage],
  );

  useEffect(() => {
    const controller = new AbortController();
    void fetchLogs(controller.signal);
    return () => controller.abort();
  }, [fetchLogs]);

  const handleChangePage = (_: unknown, newPage: number) => {
    [appliedFilters, graphqlEndpoint, page, rowsPerPage],
  };

  const handleChangeRowsPerPage = (event: ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const serviceOptions = useMemo(() => {
    const unique = new Set<string>();
    logPage?.items.forEach((entry) => {
      if (entry.serviceName) {
        unique.add(entry.serviceName);
      }
    });
    return Array.from(unique).sort((a, b) => a.localeCompare(b));
  }, [logPage]);

  const activeFilters = useMemo(() => {
    return (Object.entries(appliedFilters) as [FilterKey, string][])
      .filter(([, value]) => value.trim().length > 0);
  }, [appliedFilters]);

  const rows: LogEntry[] = logPage?.items ?? [];

  return (
    <Container maxWidth="xl" className="admin-dashboard-container">
      <Stack spacing={3}>
        <Stack
          direction={{ xs: 'column', md: 'row' }}
          justifyContent="space-between"
          alignItems={{ xs: 'flex-start', md: 'center' }}
          spacing={2}
        >
          <Box>
            <Typography variant="overline" color="text.secondary">
              Centro de monitoreo
            </Typography>
            <Stack direction="row" alignItems="center" spacing={1}>
              <ManageSearchRoundedIcon color="primary" />
              <Typography variant="h4">Visor de logs (GraphQL)</Typography>
            </Stack>
            <Typography variant="body2" color="text.secondary">
              Consulta en tiempo real los eventos capturados por el colector gRPC.
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Endpoint GraphQL: {graphqlEndpoint}
            </Typography>
          </Box>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} width={{ xs: '100%', sm: 'auto' }}>
            <Button variant="text" onClick={() => navigate('/dashboard')}>
              Volver al home banking
            </Button>
            <Tooltip title="Actualizar">
              <span>
                <IconButton onClick={() => void fetchLogs()} disabled={status === 'loading'}>
                  <RefreshRoundedIcon />
                </IconButton>
              </span>
            </Tooltip>
          </Stack>
        </Stack>

        <Paper className="admin-content" elevation={4}>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Box>
                <Typography variant="h6">Eventos recientes</Typography>
                <Typography variant="body2" color="text.secondary">
                  Página {logPage?.page ?? page + 1} · Tamaño {logPage?.pageSize ?? rowsPerPage}
                </Typography>
              </Box>
              <Stack spacing={1} alignItems="flex-end">
                <Typography variant="subtitle2">Administrador:</Typography>
                <Typography variant="body1" fontWeight={600}>
                  {auth.profile?.nombre ?? 'Usuario'}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {auth.profile?.email}
                </Typography>
              </Stack>
            </Stack>

            <Divider />

            <Paper className="admin-filters-panel" elevation={0} variant="outlined">
              <Stack spacing={2}>
                <Stack direction="row" alignItems="center" spacing={1}>
                  <FilterAltRoundedIcon color="secondary" />
                  <Typography variant="subtitle1">Filtros dinámicos</Typography>
                </Stack>
                <Stack
                  direction={{ xs: 'column', lg: 'row' }}
                  spacing={2}
                  flexWrap="wrap"
                  useFlexGap
                >
                  <FormControl size="small" fullWidth>
                    <InputLabel id="service-filter-label">Servicio</InputLabel>
                    <Select
                      labelId="service-filter-label"
                      label="Servicio"
                      value={filters.serviceName}
                      onChange={(event) => updateFilter('serviceName', event.target.value)}
                    >
                      <MenuItem value="">Todos</MenuItem>
                      {serviceOptions.map((service) => (
                        <MenuItem key={service} value={service}>
                          {service}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                  <FormControl size="small" fullWidth>
                    <InputLabel id="severity-filter-label">Severidad</InputLabel>
                    <Select
                      labelId="severity-filter-label"
                      label="Severidad"
                      value={filters.severity}
                      onChange={(event) => updateFilter('severity', event.target.value)}
                    >
                      <MenuItem value="">Todas</MenuItem>
                      {severityOptions.map((option) => (
                        <MenuItem key={option} value={option}>
                          {option}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                  <TextField
                    size="small"
                    fullWidth
                    label="Ruta"
                    placeholder="/api/..."
                    value={filters.requestPath}
                    onChange={(event) => updateFilter('requestPath', event.target.value)}
                  />
                  <TextField
                    size="small"
                    fullWidth
                    label="Correlación"
                    placeholder="Trace o GUID"
                    value={filters.correlationId}
                    onChange={(event) => updateFilter('correlationId', event.target.value)}
                  />
                  <TextField
                    size="small"
                    fullWidth
                    label="Texto"
                    placeholder="Mensaje, payload o stack"
                    value={filters.search}
                    onChange={(event) => updateFilter('search', event.target.value)}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <ManageSearchRoundedIcon fontSize="small" />
                        </InputAdornment>
                      ),
                    }}
                  />
                </Stack>
                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button variant="text" onClick={handleResetFilters} disabled={status === 'loading'}>
                    Limpiar
                  </Button>
                  <Button variant="contained" onClick={handleApplyFilters} disabled={status === 'loading'}>
                    Aplicar filtros
                  </Button>
                </Stack>
                {activeFilters.length > 0 && (
                  <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                    {activeFilters.map(([key, value]) => (
                      <Chip
                        key={key}
                        label={`${filterLabels[key]}: ${truncate(value, 24)}`}
                        size="small"
                        onDelete={() => handleRemoveFilter(key)}
                        color="info"
                      />
                    ))}
                  </Stack>
                )}
              </Stack>
            </Paper>

            <Divider />

            {status === 'error' && error && <Alert severity="error">{error}</Alert>}

            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell width="15%">Fecha</TableCell>
                    <TableCell width="15%">Servicio</TableCell>
                    <TableCell width="10%">Severidad</TableCell>
                    <TableCell>Mensaje</TableCell>
                    <TableCell width="15%">Ruta</TableCell>
                    <TableCell width="15%">Correlación</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {status === 'loading' && rows.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={6} align="center">
                        <Stack direction="row" spacing={1} alignItems="center" justifyContent="center">
                          <CircularProgress size={18} />
                          <Typography variant="body2" color="text.secondary">
                            Consultando GraphQL...
                          </Typography>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  )}

                  {rows.length === 0 && status !== 'loading' && (
                    <TableRow>
                      <TableCell colSpan={6} align="center">
                        <Typography variant="body2" color="text.secondary">
                          No se encontraron eventos en esta página.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}

                  {rows.map((entry) => (
                    <TableRow key={entry.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={600}>
                          {formatDateTime(entry.timestampUtc)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={entry.serviceName} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={entry.severity}
                          size="small"
                          color={severityToColor(entry.severity)}
                          variant="filled"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{entry.message}</Typography>
                        {entry.payload && (
                          <Typography variant="caption" color="text.secondary">
                            Payload: {truncate(entry.payload, 120)}
                          </Typography>
                        )}
                        {entry.stackTrace && (
                          <Typography variant="caption" color="text.secondary">
                            Stack: {truncate(entry.stackTrace, 120)}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {entry.requestPath || 'N/D'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {entry.correlationId ? (
                          <Chip label={truncate(entry.correlationId, 24)} size="small" variant="outlined" />
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            Sin correlación
                          </Typography>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={logPage?.totalCount ?? 0}
              page={page}
              rowsPerPage={rowsPerPage}
              onPageChange={handleChangePage}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[10, 25, 50, 100]}
            />
          </Stack>
        </Paper>
      </Stack>
    </Container>
  );
};

const buildGraphqlEndpoint = (baseUrl: string) => {
  const trimmed = baseUrl.replace(/\/+$/, '');
  if (trimmed.toLowerCase().endsWith('/graphql')) {
    return trimmed;
  }
  return `${trimmed}/graphql`;
};

const severityToColor = (severity: string): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
  const normalized = severity.toLowerCase();
  if (normalized.includes('fatal') || normalized.includes('error') || normalized.includes('critical') || normalized.includes('crit')) {
    return 'error';
  }
  if (normalized.includes('warn')) {
    return 'warning';
  }
  if (normalized.includes('info')) {
    return 'info';
  }
  if (normalized.includes('debug') || normalized.includes('trace')) {
    return 'secondary';
  }
  if (normalized.includes('success')) {
    return 'success';
  }
  return 'primary';
};

const formatDateTime = (value: string) => {
  try {
    return new Date(value).toLocaleString();
  } catch (error) {
    return value;
  }
};

const truncate = (value: string, max = 120) => {
  if (value.length <= max) {
    return value;
  }
  return `${value.slice(0, max)}...`;
};

const extractGraphqlError = (error: unknown, fallback = 'No pudimos consultar los logs.') => {
  if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
};

export default AdminDashboard;
