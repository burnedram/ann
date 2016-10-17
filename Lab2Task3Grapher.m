function outfile = Lab2Task3Grapher(task3file, filePattern, ks)
fileID = fopen(task3file, 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%d %f %f'; %class x y
A = fscanf(fileID, spec, [3 Inf])';
fclose(fileID);

B = csvread(sprintf(filePattern, 'k'));
kohonens = cell(1, length(ks));
boundaries = cell(1, length(ks));
for i = 1:length(ks)
    kohonens{i} = csvread(sprintf(filePattern, sprintf('%s_%d', 'kohonen', ks(i))));
    boundaries{i} = csvread(sprintf(filePattern, sprintf('%s_%d', 'boundary', ks(i))));
end

for i = 1:length(ks)
    [path, name, ~] = fileparts(sprintf(filePattern, sprintf('%d', ks(i))));
    outfile = sprintf('%s.png', name);
    outname = outfile;
    if path ~= ''
        outfile = strcat([path, filesep, outfile]);
    end
    h = figure('Name', outname, 'NumberTitle', 'off');
    hold on;
    scatter(A(A(:, 1) == 1, 2), A(A(:, 1) == 1, 3), '.');
    scatter(A(A(:, 1) == -1, 2), A(A(:, 1) == -1, 3), '.');
    scatter(kohonens{i}(:, 1), kohonens{i}(:, 2), 'o', 'MarkerEdgeColor', 'black', 'MarkerFaceColor', 'black');
    plot(boundaries{i}(:, 1), boundaries{i}(:, 2), 'black');
    title(sprintf('Hybrid learning algorithm, k = %d, Avg(Validation_{error}) = %0.3f', ks(i), B(B(:, 1) == ks(i), 2)));
    legend({'Class = 1', 'Class = -1', 'Kohonen weights', 'Decision boundary'});
    saveas(h, outfile);
end

[path, name, ~] = fileparts(sprintf(filePattern, 'k'));
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
plot(B(:, 1), B(:, 2), 'black');
title('Hybrid learning algorithm performance');
xlabel('k');
ylabel('Avg(Validation_{error})');
saveas(h, outfile);
end