function outfile = Task1Grapher(file)
fileID = fopen(file, 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %d %d'; %p/N, errorRate, p, N
A = fscanf(fileID, spec, [4 Inf])';
fclose(fileID);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

x = A(:, 1);
y = A(:, 2);

xtheo = min(x):0.01:max(x);
ytheo = 1/2 * (1 - erf(sqrt(1./(2.*xtheo))));

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
scatter(x, y, '.');
plot(xtheo, ytheo);
legend({'Computed error rates', 'Theoretical curve'}, 'Location', 'Best');
xlabel('p/N');
ylabel('P_{error}');
title('One-step error probability');
saveas(h, outfile);
end