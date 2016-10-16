% See http://se.mathworks.com/help/curvefit/list-of-library-models-for-curve-and-surface-fitting.html#btbcvnl
% for what fittype can be
function outfile = Task1Extra(file, fittype)
fileID = fopen(file, 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %d %d'; %p/N, errorRate, p, N
A = fscanf(fileID, spec, [4 Inf])';
fclose(fileID);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s_%s.png', name, fittype);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

x = A(:, 1);
y = A(:, 2);
f = fit(x, y, fittype); % Curve fitting toolbox
stdmaxx = max(x);
maxfindx = stdmaxx * 100;

% Calculate x limits
eqone = findRoots(f, 1, stdmaxx, maxfindx);
eqonereal = eqone(imag(eqone) == 0);
eqzero = findRoots(f, 0, stdmaxx, maxfindx);
eqzeroreal = eqzero(imag(eqzero) == 0);
eqalmostzero = findRoots(f, realmin, stdmaxx, maxfindx);
eqalmostzeroreal = eqalmostzero(imag(eqalmostzero) == 0);
if ~isempty(eqonereal)
    maxx = min(eqonereal);
elseif ~isempty(eqzeroreal)
    maxx = min(eqzeroreal);
elseif ~isempty(eqalmostzeroreal)
    maxx = min(eqalmostzeroreal);
elseif ~isempty(eqone)
    maxx = max(real(eqone));
elseif ~isempty(eqzero)
    maxx = min(real(eqzero));
elseif ~isempty(eqalmostzero)
    maxx = min(real(eqalmostzero));
else
    maxx = maxfindx;
end

h = figure('Name', outname, 'NumberTitle', 'off');
scatter(x, y, '.');
xlim([realmin, maxx]);

hold on;
plot(f);
legend({'Computed error rates', 'Fitted curve'}, 'Location', 'Best');
xlabel('p/N');
ylabel('P_{error}');
title(fittype);
saveas(h, outfile);
end

function res = findRoots(fit, eq, gt, lt)
syms x z;
f = cfitToFunc(fit);
solx = vpasolve(f == eq, x, [gt lt]);% Symbolic math toolbox
res = eval(solx);
%res = res(res > gt);
end

function f = cfitToFunc(fit)
names = coeffnames(fit);
values = coeffvalues(fit);
for fitindex = 1:length(names)
    name = names(fitindex);
    name = name{1};
    assign(name, values(fitindex));
end
syms x;
cmd = strcat(['f = ', formula(fit), ';']);
eval(repairBrokenString(cmd));
end

% Removes newlines and multiple whitespaces in a row
function str = repairBrokenString(str)
    newline = sprintf('\n');
    str = strrep(str, newline, '');
    strippedstr = strrep(str, '  ', ' ');
    while ~strcmp(str, strippedstr)
        str = strippedstr;
        strippedstr = strrep(str, '  ', ' ');
    end
end

function assign(name, value)
    assignin('caller', name, value);
end