function outfile = Task1Grapher(file, polycount)
fileID = fopen(file, 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %d %d'; %p/N, errorRate, p, N
A = fscanf(fileID, spec, [4 Inf])';
fclose(fileID);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s_%d.png', name, polycount);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

x = A(:, 1);
y = A(:, 2);
f = fit(x, y, sprintf('poly%d', polycount)); % Curve fitting toolbox
stdmaxx = max(x);
%solx = solveCfitEq(f, 1) % Symbolic math toolbox

% Calculate x limits
coeffs = coeffvalues(f);
coeffs(length(coeffs)) = coeffs(length(coeffs)) - 1;
eqone = roots(coeffs);
eqone = eqone(imag(eqone) == 0);
eqone = eqone(eqone > stdmaxx);
if ~isempty(eqone)
    maxx = min(eqone);
else
    extremes_x = roots(polyder(coeffvalues(f)));
    extremes_x = extremes_x(extremes_x > stdmaxx);
    if isempty(extremes_x)
        maxx = stdmaxx;
    else
        extremes_y = f(extremes_x);
        [~, extremes_i] = max(extremes_y);
        maxx  = extremes_x(extremes_i);
    end
end

h = figure('Name', outname, 'NumberTitle', 'off');
scatter(x, y, '.');
xlim([0, maxx]);

hold on;
plot(f);
legend({'Computed error rates', 'Fitted curve'}, 'Location', 'Best');
xlabel('p/N');
ylabel('P_e_r_r_o_r');
title(sprintf('Fitted on a polynomial with degree %d', polycount));
saveas(h, outfile);
end

function res = solveCfitEq(fit, eq)
syms x;
f = cfitToFunc(fit);
solx = solve(f == eq, x);
res = eval(solx);
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
eval(strcat(['f = ', formula(fit), ';']));
end

function assign(name, value)
    assignin('caller', name, value);
end